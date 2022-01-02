import socket, tqdm, os, urllib3

SERVER_HOST="0.0.0.0"
SERVER_PORT=4444
BUFFER_SIZE=4096
SEPARATOR= "<SEPARATOR>"
http=urllib3.PoolManager()
while True:
    s=socket.socket()
    s.bind((SERVER_HOST,SERVER_PORT))
    s.listen(10)
    print(f"[*] Listening as {SERVER_HOST}:{SERVER_PORT}")
    print("Waiting for connections...")
    client_socket, address = s.accept()
    print(f"[+] {address} is connected.")
    USER_AUTH = client_socket.recv(BUFFER_SIZE).decode()
    client_socket.send(f"{USER_AUTH}".encode())
    #a=http.request("GET", f"https://localhost:8080/auth/userauthho?hash={USER_AUTH}")
    if False:#(a.data.decode() != "user_authentic"):
        print("[+] Client provided invalid credentials; client removed")
        s.close()
    else:
        while True:
            try:
                received = client_socket.recv(BUFFER_SIZE).decode()
                filename, filesize = received.split(SEPARATOR)
                filename = os.path.basename(filename)
                filesize = int(filesize)
            except ValueError:
                client_socket.send(b"<CONFIRMFILETRANSFERCOMPLETE>")
                if client_socket.recv(BUFFER_SIZE) == b"<FILETRANSFERCOMPLETE>":
                    break
                else:
                    print("uh oh")
            print(f"Receiving {filename}")
            with open("incoming\\"+filename, "wb") as f:
                while True:
                    bytes_read = client_socket.recv(BUFFER_SIZE)
                    if not bytes_read:
                        break
                    elif b"<FILEBREAK>" in bytes_read:
                        client_socket.send(b"<FILEBREAK>")
                        break
                    else:
                        f.write(bytes_read)
        client_socket.close()
        s.close()
