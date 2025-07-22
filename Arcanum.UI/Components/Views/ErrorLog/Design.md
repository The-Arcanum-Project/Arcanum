# Design of the ErrorLogView

## Windows
- ErrorLogView
- FileStatusView

### ErrorLogView
- Displays a list of all recorded errors.
- Has extensive filters and search functionality.
- Can be used to navigate to the source of the error.


### FileStatusView
- Displays the vanilla folder structure.
- We mark all folders / files we load and parse
- Every file has a status indicator:
  - FileIconWithCheckmark: File is loaded and parsed correctly.
  - FileIconWithXSymbol: File is loaded, but has warnings or parsing error.
  - FileWithCorruptedIcon: File is not loaded as it is corrupted or missing.
  - The Icons propagate up through the folder with the worst status being shown.
- Link to the errorLogView with a preset filter for the file.