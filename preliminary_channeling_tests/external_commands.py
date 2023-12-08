import socket
import random
import os
from command_pb2 import Command  # Import your generated Protobuf class

# Function to send image data
def send_image(connection, image_path):
    # Check if the image file exists
    if not os.path.isfile(image_path):
        print("Image file does not exist")
        return

    # Open the image file and send its bytes
    with open(image_path, 'rb') as image_file:
        image_data = image_file.read()
        # Send the size of the image first
        connection.sendall(len(image_data).to_bytes(4, byteorder='big'))
        # Send the image data
        connection.sendall(image_data)
    print("Image data sent")

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
        # Receive the data in small chunks and retransmit it
        data = connection.recv(1024)
        print("Signal received")
        if data:
            # Deserialize the data using protobuf to determine the type of request
            request = Command()
            request.ParseFromString(data)
            
            # Check the type of request and act accordingly
            if request.type == "RequestRandomNumber":
                # Generate a random number
                random_number = random.randint(0, 9999)
                print("Random number generated:", random_number)
                
                # Create a new Command message with the random number
                response = Command()
                response.id = 1  # Set as needed
                response.type = "RandomNumberResponse"  # Set as needed
                response.number = random_number
                
                # Serialize and send the response
                serialized_message = response.SerializeToString()
                length_prefix = len(serialized_message).to_bytes(4, byteorder='little')
                connection.send(length_prefix + serialized_message)
                print("Random number sent")

            elif request.type == "RequestRandomPicture":
                # Specify the path to your image file
                random_number = random.randint(0, 5)
                image_path = f'D:/Unity Projects/channeling_test/random_images/{random_number}.png'  
                print(image_path)
                send_image(connection, image_path)

    finally:
        # Clean up the connection
        connection.close()
