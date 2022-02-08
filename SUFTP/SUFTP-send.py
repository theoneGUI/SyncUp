import socket, os, sys, json

SEPARATOR="<SEPARATOR>"
BUFFER_SIZE=4096
try:
    if "Desktop" in os.listdir(os.environ["USERPROFILE"]+"\\OneDrive".format(os.getlogin())):
        folderToRemoveFromPath = os.environ["USERPROFILE"]+"\\OneDrive\\Desktop".format(os.getlogin())
    else:
        folderToRemoveFromPath = os.environ["USERPROFILE"]+"\\Desktop".format(os.getlogin())
except:
    folderToRemoveFromPath = os.environ["USERPROFILE"]+"\\Desktop".format(os.getlogin())
try:
    print("\n\n")
    stage1 = sys.argv[1]
    stage2 = stage1.replace(":\\\\","##").replace("{","").replace("}","").replace("'","")
    stage3 = stage2.split(",")
    filesToTransfer = {}
    for i in stage3:
        x = i.split(":")
        y = x[0].replace("##",":\\\\").replace("\\\\", "\\")
        filesToTransfer[y] = x[1]
    targetIP = filesToTransfer["TARGET_IP_ADDRESS"]
    
except IndexError:
    print("You must specify files to transfer")
    exit()
s = socket.socket()
host = "syncup.thatonetechcrew.net"
port = 4444
print(f"[+] Connecting to {host}:{port}")
s.connect((host, port))
USER_HASH="ALPHA_VERSION_PLACEBO_HASH"
print("Connected to server... sending credentials")
s.send(f"{USER_HASH}".encode())
print("Credentials sent... ")#waiting for response")
#s.recv(BUFFER_SIZE)
print("[+] Connected to", host)
for filename in filesToTransfer:
    filenameToTransfer = filename.replace(folderToRemoveFromPath, "<NEWFOLDER>")
    if filename == "TARGET_IP_ADDRESS":
        pass
    else:
        try:
            filesize = os.path.getsize(filename)
            print(f"Attempting to send {filename}...")
            s.send(f"{filenameToTransfer}{SEPARATOR}{filesize}".encode())
            print("Filename and size sent...")
            s.recv(BUFFER_SIZE)
            print(f"sending {filenameToTransfer}")
            with open(filename, "rb") as f:
                while True:
                    bytes_read = f.read(BUFFER_SIZE)
                    if not bytes_read:
                        break
                    if len(bytes_read)%4096 or len(bytes_read) == 0:
                        while len(bytes_read)%4096:
                            bytes_read += b"\00"
                    s.sendall(bytes_read)
            s.send(b"\01\01\01\01")
            s.recv(BUFFER_SIZE)
            s.send(b"<FILEBREAK>")
        except PermissionError:
            pass
s.send(b"<FILETRANSFERCOMPLETE>")
s.recv(BUFFER_SIZE)
s.send(b"<FILETRANSFERCOMPLETE>")
s.close()
print("file transfer complete")


