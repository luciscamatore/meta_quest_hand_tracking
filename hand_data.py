import socket
import json

def start_server(host='127.0.0.1', port=5005):
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((host, port))
    server_socket.listen(5)
    print(f"Server listening on {host}:{port}")

    while True:
        client_socket, client_address = server_socket.accept()
        print(f"Connection from {client_address}")

        try:
            buffer = ""
            while True:
                data = client_socket.recv(1024).decode('utf-8')
                if not data:
                    break

                buffer += data
                while "\n" in buffer:
                    line, buffer = buffer.split("\n", 1)
                    
                    try:
                        data_dict = json.loads(line)
                        pretty = json.dumps(data_dict, indent=4)
                        print("Received Data: ")
                        print(pretty)
                    except json.JSONDecodeError as e:
                        print("Invalid JSON received: {e}")
                        
        except Exception as e:
            print(f"Error: {e}")
        finally:
            client_socket.close()
            print(f"Connection with {client_address} closed.")

if __name__ == "__main__":
    start_server()