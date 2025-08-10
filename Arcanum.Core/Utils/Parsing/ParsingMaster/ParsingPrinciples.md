## How do we load files from the game?
We have 4 objects holding data about the files:
- `FileInformation`: Contains default fileName, ifItIsOverridable and a `FileDescriptor`
- `FileDescriptor`: Represents one type of file, e.g. `LocationDefinition`, `Location.png`, `ReligionDefinitions`... Contains the local path tho the folder containing the defined files. Hold loading and saving service for the file Type as well as an instance to all loaded files in the form of `FileObj`s
- `FileObj`: For each loaded file, we create a `FileObj` which has a reference to the `FileDescriptor`, the matching `PathObj` and a list of `ISaveable` objects which are parsed from the file. Also defines if multiple instances of this fileType are allowed.
- `PathObj`: Represents the path to the file by containing a `DataSpace`, a `localPath` and the fileName

## Common Loading interface:
Each Descriptor provides a method to load a single FileObj => HotReloading
Each Descriptor provides a method to unload all data parsed from a FileObj => HotReloading

## Main Loading:
All loading processes are derivatives of the BaseFileLoader
### BaseFileLoader (abstract):
Data parsed from a file is only merged into the global data once the entire step if completed.
- Included Functionalities
    - Time Measurement
    - Parallel Loading option
    - Time Estimation
    - Event feedback for SubStepCompleted, StepCompleted, LoadingPercentageChanged; only StepCompleted is mandatory
- Abstract Methods:
    - bool LoadFileData(FileObj)
    - FileInformation CreateFileInformation()
    - List\<FileDescriptor\> CreateFileDescriptors(FileInformation)
    - List\<FileObj\> CreateFileObjects(FileDescriptor)