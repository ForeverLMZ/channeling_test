import socket
import random
from command_pb2 import Command  # Import your generated Protobuf class

# Set up a TCP/IP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Bind the socket to a public host, and a well-known port
sock.bind(("192.168.0.118", 4242))  

# Activate the server; this will keep running until you interrupt the program with Ctrl+C
sock.listen(1)
print("Server listening...")

while True:
    # Accept new connections
    connection, client_address = sock.accept()
    try:
        while True:
            # Receive the data in small chunks and retransmit it
            data = connection.recv(1024)
            print("signal received")
            if data:
                # Generate a random number
                random_number = random.randint(0, 9999)
                print("random number generated")
                # Create a new Command message with the random number
                response = Command()
                response.id = 1  # Example value, set as needed
                response.type = "RequestRandomNumber"  # Example value, set as needed
                response.number = random_number  # Make sure the 'number' field exists in your .proto file
                print("command created")
                # Serialize the message and prepend the length

                serialized_message = response.SerializeToString()
                length_prefix = len(serialized_message).to_bytes(4, byteorder='little')
                
                connection.send(length_prefix + serialized_message)
                break
    finally:
        # Clean up the connection
        connection.close()
