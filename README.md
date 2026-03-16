**AI Usage Notice**: this project was almost entirely made by AI, as a two-hours project to help with some challenges at work. I take no responsibility for the code quality or safety, as I prioritized speed and getting MVP as fast as possible. 
This software is provided as-is, without any warranty or support. Use it for whatever purpose you like, commercial included. 
I may choose to update it in the future, but no promises.

---

# ⚙ SynCraft

**SynCraft** is a lightweight, self-hosted process management tool built with ASP.NET Core Razor Pages and SQLite. It helps teams define repeatable process templates, launch instances of those processes with a target date, and track every step through to completion — all visualized on an interactive timeline.

Think of it as a simple, focused alternative to heavyweight project management tools — designed for coordinating recurring multi-step workflows like software releases, event planning, compliance audits, or any process that follows a predictable pattern.

---

## ✨ Key Features

### 📋 Process Templates
Define reusable process blueprints that describe the steps required to complete a workflow.

- Each template has a **name**, **description**, and optional **target date comment** (a hint shown when launching instances, e.g. "Date of the event")
- **Steps** are defined with:
  - Name and description
  - **Category** — Development, Documentation, Security, Publishing, or Tests (color-coded throughout the UI)
  - **Responsible person** — who should own this step
  - **Day offset** — when the step is due relative to the target date (e.g. `-7` = 7 days before target)
  - **Minimum duration** — how many days the step needs, used to calculate a "must start by" date
- **Milestones** — labeled markers on the timeline with a day offset and color (Green, Blue, or Red)
- Inline editing of steps directly on the template edit page
- Templates can be deleted with confirmation

### 🚀 Process Instances
Launch a process from any template by picking a target date. All template steps are copied into the instance with their deadlines computed automatically.

- Each instance tracks its own **target date**, **status** (Active / Completed), and **progress** (done steps / total)
- Steps can be moved through states: **Not Started → In Progress → In Risk → Done / Cancelled**
- **Automatic risk detection:**
  - **Delayed** — past deadline and not done/cancelled (pulsing red indicator)
  - **Should Have Started** — past the "must start by" date and still not started (yellow warning)
  - **At Risk** — in progress but not enough days remain before the deadline to complete the required duration
- **Mark Complete** — available when all steps are done or cancelled
- **Push Deadline** — push the target date and all active steps forward by N days. Done/cancelled steps keep their original deadlines. A red milestone is automatically created at the original target date.
- Instances can be deleted with confirmation

### 📊 Interactive Timeline
Every process instance page features a visual timeline that provides an at-a-glance view of the entire process.

- **Pin markers** — each step appears as a vertical signpost at its deadline position, showing its category badge and name
- **Duration bars** — steps with a minimum duration show a horizontal bar extending left from the deadline to the "must start by" date
- **Lane stacking** — overlapping steps for the same person are automatically stacked into separate lanes
- **Rows per person** — the timeline is grouped by responsible person
- **Today marker** — blue vertical line with label above the timeline
- **Target date marker** — thick red vertical line (always visible)
- **Milestone markers** — colored vertical lines (green/blue/red) with labels, spanning all rows
- **Hover to front** — overlapping labels can be hovered to bring them to the foreground
- **Click to inspect** — clicking any pin opens a detailed Bootstrap modal showing step info, state change buttons, and comments
- Color-coded states throughout: done (green), in progress (blue), at risk / delayed (red), cancelled (grey), warning (yellow)

### 👥 Persons
Manage the people involved in your processes.

- Each person has a **name**, optional **email**, and optional **role**
- Persons are assigned to template steps and carried over to instances
- Full CRUD (create, edit, delete)

### 📌 Steps by Person
A dedicated view showing all open (non-done, non-cancelled) steps across all active processes, grouped by person.

- Quickly see who is overloaded, who has delayed tasks, and what's coming up
- Links directly to the relevant process instance

### 🏠 Dashboard
The home page provides a high-level summary:

- Cards showing counts of active processes, completed processes, templates, and persons
- All process instances grouped by template, with:
  - Progress bar (done/total steps)
  - Status badges (Active, Delayed, Completed)
  - Currently in-progress steps
  - Direct link to each instance's timeline view

### 💾 Export / Import
Full data portability via JSON:

- **Export** — download all persons, templates (with steps & milestones), and process instances (with steps, states & comments) as a single JSON file
- **Import** — upload a previously exported JSON file. Duplicate persons and templates (matched by name) are skipped; instances are linked to templates by name
- Useful for backups, migrating between environments, or sharing template libraries

---

## 🛠 Tech Stack

| Component       | Technology                          |
|-----------------|-------------------------------------|
| Framework       | ASP.NET Core Razor Pages (.NET 10)  |
| Language        | C# 14                               |
| Database        | SQLite via EF Core                   |
| Frontend        | Bootstrap 5, jQuery, vanilla JS      |
| Authentication  | None (designed for local/team use)   |

---

## 🚀 Getting Started

### Windows x64

Under `dist\win-64\` you can find a built version for Windows x64.

#### Run

```bash
cd SynCraft
SynCraft.exe
```

The app will create a `syncraft.db` SQLite database automatically on first launch. No migrations or external database setup required.

Navigate to `https://localhost:<port>` (the port is shown in the console output).

### Linux x64

Under `dist\linux-64\` you can find a built version for Linux x64.

#### Run

```bash
cd SynCraft
chmod +x SynCraft
./SynCraft
```

The app will create a `syncraft.db` SQLite database automatically on first launch. No migrations or external database setup required.

Navigate to `https://localhost:<port>` (the port is shown in the console output).


### Any Platform

Under `dist\any-platform\` you can find a cross-platform built version.

#### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later

#### Run

```bash
cd SynCraft
dotnet run
```

The app will create a `syncraft.db` SQLite database automatically on first launch. No migrations or external database setup required.

Navigate to `https://localhost:<port>` (the port is shown in the console output).

## First Steps

1. **Add persons** — go to _Persons_ and create the people involved in your processes
2. **Create a template** — go to _Templates_, create one, then add steps (with day offsets relative to the target date) and optionally milestones
3. **Launch a process** — go to _Processes_ → _Launch New Process_, pick a template and set a target date
4. **Track progress** — open the process to see the timeline, update step states, add comments, and push deadlines if needed

---

## 📁 Project Structure

```
SynCraft/
├── Data/
│   └── SynCraftDbContext.cs        # EF Core DbContext
├── Models/
│   ├── Enums.cs                    # StepCategory, StepState, ProcessStatus, MilestoneColor
│   ├── Person.cs                   # Person entity
│   ├── ProcessTemplate.cs          # Process template with Steps & Milestones
│   ├── StepTemplate.cs             # Step blueprint (day offset, duration, category, etc.)
│   ├── MilestoneTemplate.cs        # Milestone blueprint (day offset, label, color)
│   ├── ProcessInstance.cs           # Running process with target date & status
│   ├── StepInstance.cs              # Step in a running process (computed Deadline, IsAtRisk, etc.)
│   └── StepComment.cs              # Comment on a step instance
├── Pages/
│   ├── Index.cshtml                # Dashboard
│   ├── Data/
│   │   └── Index.cshtml            # Export / Import page
│   ├── Instances/
│   │   ├── Index.cshtml            # Process instances list
│   │   ├── Create.cshtml           # Launch new process from template
│   │   └── View.cshtml             # Timeline view with full process management
│   ├── Persons/
│   │   ├── Index/Create/Edit/Delete  # Person CRUD
│   ├── Steps/
│   │   └── ByPerson.cshtml         # Open steps grouped by person
│   └── Templates/
│       ├── Index.cshtml            # Templates list
│       ├── Create.cshtml           # New template
│       └── Edit.cshtml             # Edit template with inline step & milestone management
├── wwwroot/css/
│   └── site.css                    # Timeline styles, pin markers, milestone markers, etc.
├── Program.cs                      # App startup, DB initialization
└── SynCraft.csproj                 # Project file
```

---

## 🔑 Core Concepts

### Day Offsets
Steps and milestones use **day offsets** relative to the process target date:
- `-14` → 14 days **before** the target date
- `0` → **on** the target date
- `+3` → 3 days **after** the target date

When you launch a process with a target date, all deadlines are computed automatically.

### Minimum Duration
Steps can specify a minimum duration (e.g. 5 days). The system computes a "must start by" date (`deadline - duration`) and will:
- Warn if the step hasn't started yet but should have
- Flag in-progress steps as "at risk" if not enough days remain

### Push Deadline
When plans change, you can push the target date forward by N days:
- All active steps (not done/cancelled) shift with it
- Done/cancelled steps keep their original deadlines
- A red "Original Target" milestone is created at the old target date

---

## 📦 Data Portability

Export and import via _Export / Import_ in the navigation bar.

The JSON export format uses human-readable names (not database IDs) for all references, enum values are stored as strings, and the file is indented for readability. Example structure:

```json
{
  "ExportDate": "2025-07-15T12:00:00Z",
  "Persons": [
    { "Name": "Alice", "Email": "alice@example.com", "Role": "Developer" }
  ],
  "Templates": [
    {
      "Name": "Software Release",
      "Steps": [
        { "Name": "Code Freeze", "Category": "Development", "DayOffset": -14, ... }
      ],
      "Milestones": [
        { "Label": "Feature Cutoff", "DayOffset": -21, "Color": "Blue" }
      ]
    }
  ],
  "Instances": [
    {
      "Name": "v2.0 Release",
      "TemplateName": "Software Release",
      "TargetDate": "2025-08-01T00:00:00",
      "Status": "Active",
      "Steps": [ ... ]
    }
  ]
}
```

---

## 📄 License

This project is distributed under the MIT license, and can be used freely and commercially.
