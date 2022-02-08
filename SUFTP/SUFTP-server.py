import socket, os, urllib3

SERVER_HOST="0.0.0.0"
SERVER_PORT=4444
BUFFER_SIZE=4096
SEPARATOR= "<SEPARATOR>"
http=urllib3.PoolManager()
try:
    if "Desktop" in os.listdir(os.environ["USERPROFILE"]+"\\OneDrive".format(os.getlogin())):
        folderToAddToPath = os.environ["USERPROFILE"]+"\\OneDrive\\Desktop".format(os.getlogin())
    else:
        folderToAddToPath = os.environ["USERPROFILE"]+"\\Desktop".format(os.getlogin())
except:
    folderToAddToPath = os.environ["USERPROFILE"]+"\\Desktop".format(os.getlogin())
CWD=os.getcwd()
if True:
    s=socket.socket()
    s.bind((SERVER_HOST,SERVER_PORT))
    s.listen(10)
    print(f"[*] Listening as {SERVER_HOST}:{SERVER_PORT}")
    print("Waiting for connections...")
    client_socket, address = s.accept()
    print(f"[+] {address} is connected.")
    USER_AUTH = client_socket.recv(BUFFER_SIZE).decode()
    client_socket.send(f"{USER_AUTH}".encode())
    #if "incoming" in os.listdir():
    #    pass
    #else:
    #    os.mkdir("incoming")
    #a=http.request("GET", f"https://localhost:8080/auth/userauthho?hash={USER_AUTH}")
    if False:#(a.data.decode() != "user_authentic"):
        print("[+] Client provided invalid credentials; client removed")
        s.close()
    else:
        while True:
            try:
                received = client_socket.recv(BUFFER_SIZE).decode()
                filename, filesize = received.split(SEPARATOR)
                #filename = os.path.basename(filename)
                filename = filename.replace("<NEWFOLDER>", folderToAddToPath)
                filesize = int(filesize)
                client_socket.send(b"<ENDFILEMETADATA>")
            except ValueError:
                client_socket.send(b"<CONFIRMFILETRANSFERCOMPLETE>")
                if client_socket.recv(BUFFER_SIZE) == b"<FILETRANSFERCOMPLETE>":
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
                        bytes_read = client_socket.recv(BUFFER_SIZE)
                        bytes_read = bytes_read.replace(b"\00",b"")
                        if not bytes_read:
                            break
                        elif b"\01\01\01\01" in bytes_read:
                            client_socket.send(b"<FILEBREAK>")
                            if client_socket.recv(BUFFER_SIZE) == b"<FILEBREAK>":
                                print(f"Finished writing {filename}")
                                break
                        else:
                            f.write(bytes_read)
        client_socket.close()
        s.close()
        print("Sockets closed... server shut down")
