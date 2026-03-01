# AST Comment Upgrade

This system upgrades Arcanums serialization pipeline from simple string stitching to a full **Object -> AST -> File**
round-trip. This ensures that when mod files are modified programmatically, user comments and formatting structures are
preserved as much as possible.

## What IS Preserved

### 1. Property Comments

Comments attached to standard fields, boolean flags, or simple assignments.

```paradox
# [Leading] This controls the weather
winter = severe # [Inline] Warning: High attrition
```

### 2. Block/Object Comments

Comments attached to complex objects or list definitions.

```paradox
# [Leading] Modifier definitions
unit_modifiers = {
    discipline = 0.5
} # [Closing] End of modifiers
```

### 3. Structural/Body Comments

Comments found at the end of blocks, file headers, or standalone sections.

```paradox
climate = {
    color = { 10 10 10 }

    # [Body] TODO: Add precipitation data later
    # [Body] This section is work-in-progress
}
```

### 4. "Shattered" List Comments

Comments on lists that are split across multiple definitions are merged on the parent object, as shattered entires are
combined.

```paradox
# [Leading 1]
modifiers = { ... }
# [Leading 2] (Preserved and merged with Leading 1)
modifiers = { ... }
```

---

## What is NOT Preserved (Limitations)

### 1. Simple Collection Item Comments

Comments attached specifically to **Value Types** (Strings, Numbers, Enums) inside a list are dropped to maintain
performance.

```paradox
ruler_traits = {
    "brave" # This comment is LOST
    "diligent"
}
```