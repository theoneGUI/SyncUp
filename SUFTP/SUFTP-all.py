import sys, socket, os, urllib3,json
executable=sys.argv[1]
############################################################################################################################### SUFTP-recv.py
try:
    with open(r"C:\Program Files\SyncUp\settings.config") as file:
        a=file.readlines()
    settings={}
    for i in a:
        settings[i.split("=")[0]] = i.split("=")[1].rstrip()
    USER_AUTH=settings["USERHASH"]
    SERVER=settings["SERVER"]
    PORT=4444
except:
    print("There is a fatal error in your settings.config file. This must be corrected for SyncUp to continue.")
    raise("ConfigError")
    
try:
    folderToAddToPath=settings["SYNCFOLDER"]
    folderToRemoveFromPath=settings["SYNCFOLDER"]
except KeyError:
    try:
        if "Desktop" in os.listdir(os.environ["USERPROFILE"]+"\\OneDrive".format(os.getlogin())):
            folderToAddToPath = os.environ["USERPROFILE"]+"\\OneDrive\\Desktop".format(os.getlogin())
            folderToRemoveFromPath = os.environ["USERPROFILE"]+"\\OneDrive\\Desktop".format(os.getlogin())
        else:
            folderToAddToPath = os.environ["USERPROFILE"]+"\\Desktop".format(os.getlogin())
            folderToRemoveFromPath = os.environ["USERPROFILE"]+"\\OneDrive\\Desktop".format(os.getlogin())
    except:
        folderToAddToPath = os.environ["USERPROFILE"]+"\\Desktop".format(os.getlogin())
        folderToRemoveFromPath = os.environ["USERPROFILE"]+"\\Desktop".format(os.getlogin())
if executable=="recv":
    BUFFER_SIZE=4096
    SEPARATOR= "<SEPARATOR>"
    http=urllib3.PoolManager()
    s = socket.socket()
    CWD=os.getcwd()
    if True:
        print(f"[*] Connecting to {SERVER}:{PORT}")
        s.connect((SERVER, PORT))
        print("[*] Connected")
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
                    if not os.path.isdir(filename.replace(os.path.basename(filename),"")):
                        os.mkdir(filename.replace(os.path.basename(filename),""))
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
                            bytes_read.replace(b"\00",b"")
                            if not bytes_read:
                                break
                            elif b"\01\01\01\01" in bytes_read:
                                s.send(b"<FILEBREAK>")
                                if s.recv(BUFFER_SIZE) == b"<FILEBREAK>":
                                    print(f"Finished writing {filename}")
                                    break
                            else:
                                f.write(bytes_read.replace(b"\00",b""))
            s.close()
            print("Sockets closed... shut down")
############################################################################################################################### SUFTP-send.py code
elif executable=="send":
    SEPARATOR="<SEPARATOR>"
    BUFFER_SIZE=4096
    try:
        stage1 = sys.argv[2]
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
        quit()
    s = socket.socket()
    print(f"[+] Connecting to {SERVER}:{PORT}")
    s.connect((SERVER, PORT))
    print("Connected to server... sending credentials")
    s.send(f"{USER_AUTH}".encode())
    print("Credentials sent... ")#waiting for response")
    #s.recv(BUFFER_SIZE)
    print("[+] Connected to", SERVER)
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
############################################################################################################################### SUFTP-client.py code
elif executable=="client":
    SEPARATOR="<SEPARATOR>"
    BUFFER_SIZE=4096
    try:
        print("\n\n")
        stage1 = sys.argv[2]
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
        raise("FileTransferError")
    try:
        s = socket.socket()
        host = targetIP
        port = 4444
        print(f"[+] Connecting to {host}:{port}")
        s.connect((host, port))
        s.send(f"{USER_AUTH}".encode())
        s.recv(BUFFER_SIZE)
        print("[+] Connected to", host)
    except TimeoutError:
        print("LAN SERVER DID NOT RESPOND!")
        raise("FileTransferError")
    for filename in filesToTransfer:
        filenameToTransfer = filename.replace(folderToRemoveFromPath, "<NEWFOLDER>")
        if filename == "TARGET_IP_ADDRESS":
            pass
        else:
            try:
                filesize = os.path.getsize(filename)
                s.send(f"{filenameToTransfer}{SEPARATOR}{filesize}".encode())
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
############################################################################################################################### SUFTP-server.py code

elif executable=="server":
    SERVER_HOST="0.0.0.0"
    SERVER_PORT=4444
    BUFFER_SIZE=4096
    SEPARATOR= "<SEPARATOR>"
    http=urllib3.PoolManager()
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
        if False:
            print("[+] Client provided invalid credentials; client removed")
            s.close()
        else:
            while True:
                try:
                    received = client_socket.recv(BUFFER_SIZE).decode()
                    filename, filesize = received.split(SEPARATOR)
                    filename = filename.replace("<NEWFOLDER>", folderToAddToPath)
                    if not os.path.isdir(filename.replace(os.path.basename(filename),"")):
                        os.mkdir(filename.replace(os.path.basename(filename),""))
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
                                f.write(bytes_read.replace(b"\00",b""))
            client_socket.close()
            s.close()
            print("Sockets closed... server shut down")




