# -*- coding: utf-8 -*-
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: rollerball.proto
# Protobuf Python Version: 4.25.0
"""Generated protocol buffer code."""
from google.protobuf import descriptor as _descriptor
from google.protobuf import descriptor_pool as _descriptor_pool
from google.protobuf import symbol_database as _symbol_database
from google.protobuf.internal import builder as _builder
# @@protoc_insertion_point(imports)

_sym_db = _symbol_database.Default()




DESCRIPTOR = _descriptor_pool.Default().AddSerializedFile(b'\n\x10rollerball.proto\x12\nrollerball\"]\n\x0bObservation\x12\x12\n\nposition_x\x18\x01 \x01(\x02\x12\x12\n\nposition_y\x18\x02 \x01(\x02\x12\x12\n\nvelocity_x\x18\x03 \x01(\x02\x12\x12\n\nvelocity_y\x18\x04 \x01(\x02\"*\n\x06\x41\x63tion\x12\x0f\n\x07\x66orce_x\x18\x01 \x01(\x02\x12\x0f\n\x07\x66orce_y\x18\x02 \x01(\x02\",\n\x0cRewardSignal\x12\x0e\n\x06reward\x18\x01 \x01(\x02\x12\x0c\n\x04\x64one\x18\x02 \x01(\x08\x62\x06proto3')

_globals = globals()
_builder.BuildMessageAndEnumDescriptors(DESCRIPTOR, _globals)
_builder.BuildTopDescriptorsAndMessages(DESCRIPTOR, 'rollerball_pb2', _globals)
if _descriptor._USE_C_DESCRIPTORS == False:
  DESCRIPTOR._options = None
  _globals['_OBSERVATION']._serialized_start=32
  _globals['_OBSERVATION']._serialized_end=125
  _globals['_ACTION']._serialized_start=127
  _globals['_ACTION']._serialized_end=169
  _globals['_REWARDSIGNAL']._serialized_start=171
  _globals['_REWARDSIGNAL']._serialized_end=215
# @@protoc_insertion_point(module_scope)