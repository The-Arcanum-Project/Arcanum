Should check for any new saveables that have been added since the last save
check new saveables dictionary

Check if the path of the saveable is fixed or can be changed
check the file information in the saveable

Show all saveables with a selection of current file objects which are valid for the given saveableType
TODO How do we find the file objects that are valid for the given saveable type?

We need a list of fileObjs that are valid for the given saveableType, so we need a Dictionary of some sorts.
Might make sense to have a separate window for this, where valid files are shown and where the user can add another one.

If the saveable path is fixed, make it grayed out and only displayed with an additional checkbox

All the files are hashed when read in (maybe directly in the char by char parsing method).
Before finally saving the file, the hash is checked against the current file
If the hash is different, the file is not saved and a MergeDialog is shown to the user

The user should be shown a dialog with all the different saveableTypes
All SaveableTypes should be possible to be selected
Per default all saveable in the same file should be highlighted, and if clicked all are selected
One can override this with an additional button press (alt or ctrl) to select a single saveable
The file is read in again and only one file is changed (The base file therefore needs to be same as it was when it was
read in (see hashes))

But do we really want to separate the new saveables and the old ones?
In my opinion, it is better to have a general overview of everything that has been changed.
One can select if only added/ only modified, or all saveables should be shown
Then the user can select which saveables should be saved
This also allows users to save stuff to different files

WHAT DO WE NEED TO DO?:

1. Hash files on loading and check them before the saving popup
2. Create the Merging Dialog or at least an interface for it
3. Get all saveables that are modified or added
4. Determine files which are valid for the saving of a saveableType
5. Create and Show the dialog
    1. One should be able to select modified or added saveables
    2. Changes to the categories should only affect currently shown saveables
    3. Checkboxes will affect all the saveables in the same file per default
6. For fast saving, skip the user input except if new saveables are added

We need a list of fileObjs that are valid for the given saveableType, so we need a Dictionary of some sorts.
Might make sense to have a separate window for this, where valid files are shown and where the user can add another one.

When a new fileObj is registered, the dictionary can be constructed based on the saveable type.
Would need a dictionary of a list of all the files
Needs to update when new files are added

Merging issue:
We have a tri state: start file, end file, internal state
If the start and the end file are identical, we do not really care.
However, if we merge, we have to think about the dependencies of the files
If we, for example, change the province definitions, then the provinces have to be reloaded
But how do we handle a province which was changed? → also merging ? and continue? But then the user might generate a not
valid mod state, which we do not want.
This would be okay if we gave instant feedback, which is challenging.
At the moment implement a system which notifies the user that the file he has overridden will be overriden by us and the
user should backup the file before saving.

Saving:
Unless a git system is present, we should not directly override the data of the files. Instead, make a backup

# Functionality

- The Saveable needs information about the file it can be saved into
    - This means that some saveable cannot change the file it is saved into

- The Saveables need to be grouped by a SaveableType to allow for a better overview
    - It should be possible to only save certain SaveableTypes
    - And it should be clear which Saveable is which SaveableType

## Saving Service

- Contains the parameters which are either static or dependent on the saveables in the file for a given Descriptor
- Has the information about the footer and the header of the file
- Also contains the information about the SavingComment which is placed below the unchanged but above the changed
  saveables
    - For separation, to clearly see what has been changed
    - Can be static or dynamic, for Example, how many saveables have been changed

## Descriptors

- Should define parameters that are common across some FileObjects
    - Such as the FileType
        - Including the file extension
        - The name of the filetype
        - The comment string that is used to mark the start of a comment in the file
- Has a special SavingService that is used to save the FileObjects

## FileObjects

- Instance of a file with loaded Saveables
- Should have a reference to the Descriptor
- Has a path to the file
- Needs to be able to separate the Saveables into unchanged and changed Saveables

## Saveables

- Some object that has been loaded from a file
- Is saved in the FileObject and has a reference to the FileObject (double reference)
- Has some sort of information to determine a new FileObject if a new object of this type is created
    - Path to the file
    - Name of the file
    - If the name is fixed or can be changed

## SaveMaster

- Has a method which is called to add a new Saveable
    - When created has no FileObject but will be assigned in the saving dialog
    - Has to be able to use the DefaultFileInformation to either find or create a new FileObject
- Needs to keep track of all the Saveables that have been changed
