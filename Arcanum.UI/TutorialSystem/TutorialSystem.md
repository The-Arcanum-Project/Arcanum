# Interactive Tutorial System
An interactive tutorial system for wpf applications, designed to guide users through features and functionalities step-by-step.

## Features
- Window with description text and navigation buttons (Next, Previous, Finish).
- Optional skip button to bypass a certain chapter.
- Ability to highlight specific UI elements during the tutorial.
- All interactive features can be automatically be completed to allow skipping of parts of the tutorial.


### Tutorial Sequence
A sequence is a single Tutorial instance which contains multiple chapters.
It has a title and a description to explain what it covers.
When a tutorial sequence is started a window is opened which shows the title and the description.
It also shall show the contained chapters in a treeview, where a click on a chapter skips to it.
It also has a start and a close button on the bottom.

### Tutorial Chapter

A chapter also has a title and a description.
They also can contain chapters as subchapters, which allow endless nesting.
The chapters are shown in the same window as the sequence.
It should also be displayed how far the user is in the tutorial sequence.
There always has to be a finish button to end the tutorial.
There are two kinds of chapters:
1. StructureChapter
   - Can contain subchapters
   - Can highlight UI elements in a non-interactive way
   - Shows the description text and the contained subchapters in a treeview
2. InteractiveChapter
   - Cannot contain subchapters
   - Can contain multiple steps
   - Shows the description text
   - Shows all steps in a listview where it is clearly visible which step is currently active
   - Has buttons to skip the chapter
   - Has buttons to go to the next step (is either "skip" or "next" depending on if the step is interactive or not)
   - The listview should contain three visible slots
   - The middle slot is larger than the other two and displays additional information about the step
   - One is able to scroll through the steps where the middle slot will take the next or previous step
   - The active step is clearly marked and the listview will automatically scroll back to it if the user stopped scrolling/ interacting for a short moment.
   - There should also be a label thet shows the current step number and the total number of steps

### Tutorial Step
A step has a short title and a description.
It can be interactive or non-interactive.
An interactive step requires the user to perform a certain action in the UI.
A non-interactive step is automatically completed when the user clicks "Next".
A step can highlight multiple UI elements, but only make a single one interactive.
A step is displayed in the listview of the chapter.
The short title is shown in the listview, the description is shown in a hover tooltip.
It is clearly marked if the step was completed or not.

#### Interactive Steps
An interactive step requires the user to perform a certain action in the UI.
It can highlight multiple UI elements, but only make a single one interactive.
