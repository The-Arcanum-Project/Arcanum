# Parsing System Overview

The parsing system is meant to parse the jomini context into blocks and contents which can be processed further. The parsing system is designed to be modular and extensible, allowing for easy addition of new parsing steps as needed. 

## How to add a new parsing step
1. Create a new class that implements the `FileLoadingService` class.
2. Implement all required methods and properties.
3. Register a new `FileDescriptor` in the `ParsingMaster` and list all dependencies which should be loaded first.
4. Can be debugged when launching in debug mode using `F12` and `F9` to execute it or retrieve any generated data.