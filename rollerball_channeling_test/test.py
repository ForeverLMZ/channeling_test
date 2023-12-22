from rollerball_pb2 import Observation
observation = Observation()
byte_string = b'\r\xcd\xcc\xcc\xbd'

observation.ParseFromString(byte_string)
print("observation (X,Y,Z) received:", observation.Position.X, observation.Position.Y, observation.Position.Z )