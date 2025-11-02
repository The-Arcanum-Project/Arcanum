# Command System Specification

## Overview

The **Command System** is a core component of the **Arcanum framework**.
Its purpose is to:

* Record changes to **Nexus objects**.
* Provide **undo** and **redo** functionality.
* Support **transactions** for grouping commands together.
* Offer flexible **command stack management**, **serialization**, and **visualization**.

---

## Features

### 1. Undo/Redo Functionality

* Supports undo and redo at the **single command level**.
* After a **transaction is completed**, the **entire transaction** can be undone/redone.
* Allows **granular navigation** through individual commands within a transaction.
* Optimizations may remove certain commands during transaction processing.

---

### 2. Transactions

* Commands can be **grouped into a transaction**.
* Supports **nested transactions** for complex operations.
* Provides both:

    * **Whole-transaction undo/redo**.
    * **Step-by-step undo/redo** within a transaction (configurable).

---

### 3. Command Storage

* Commands are stored in a **tree structure**.
* Supports **branching histories**, allowing different redo paths.
* Enables **separate command stacks** for specific parts of the application:

    * Example: A command stack bound to a single window for **List modifications**.
    * These stacks are **independent** of the main command stack.

---

### 4. Serialization and Deserialization

* **Serialize command stack**:

    * Saves stack for **debugging purposes**.
    * Does **not** save the full tree, but only the **current path**.
* **Deserialize command stack**:

    * Load the previous application state at startup.
    * Restores **current path** in the stack.

---

### 5. Pruning Strategies

* Multiple strategies available to **limit stack size**.
* Prevents excessive memory or performance overhead.

---

### 6. Visualization

* Provides a **tree view** representation of the command stack.
* Highlights the **current position** in the tree.
* On **redo at a branch point**:

    * A **popup** allows user to choose which branch to follow.
    * Popup displays:

        * **Title** = next command in branch.
        * **Branch size** = number of commands in the branch.
    * User can select a branch via:

        * **Mouse click**.
        * **Mouse wheel** scrolling.
        * **Arrow keys** navigation.

---

## Summary

The **Command System** provides a flexible, transaction-aware, and visualized approach to command management in the
Arcanum framework. Its design balances **granularity**, **branching history**, and **usability**, while ensuring
debugging and persistence are supported through **serialization** and **pruning strategies**.

---

### Original

The Command System is a core component of the Arcanum framework.
It provides a way to record changes to Nexus objects, allowing for undo and redo functionality.
It also should provide a way to start a transaction, allowing multiple commands to be grouped together.
While in the interaction, undo and redo are possible but on the single command level.
After a transaction is completed, the entire transaction can be undone or redone.
A transaction can also be nested within another transaction, allowing for complex operations to be grouped together.
There also needs to be a way to only go through the individual commands within a transaction, allowing for more granular
control.
This might be dependent on the case, since some transactions might get optimized, which may result in some commands
being removed.
The Command System saves the commands as a tree structure.
However, it should also be possible to create separate command stacks for specific parts of the application.
Such as a command stack bound to a specific window where the user can modify a List.
This would make no sense to save in the main command stack, but still provides undo/redo features for the user.
The Command System should also provide a way to serialize the command stack, allowing it to be saved for debugging
purposes.
The Command System should also provide a way to deserialize the command stack.
This makes it possible to load the previous state of the application when the application is started.
However, this should not save the entire tree of the command stack, but only the current path in the tree.
The Command System should have multiple pruning strategies to limit the size of the command stack.
The Command System should also provide a way to visualize the command stack. This should be done using a tree view.
In this visualization, the user should be able to see the current position in the command stack.
When using the redo strategy, at a point where the tree branches, a small popup should appear allowing the user to
select which branch to follow.
This popup should list the branches, with the next command in the branch as the title, as well as how large the branch
is.
The user then can either click on a branch to follow it, or use the mouse wheel to scroll through the branches.
It should also be possible to use the arrow keys to navigate through the branches.
