# Task Manager

The first module: tracks personal and work tasks from creation to completion, including who's accountable to whom for them. Not an automation/scheduling tool — "Task" here means a to-do/work item, not an OS-scheduled trigger. (When a second module grows its own vocabulary, this file splits into a `CONTEXT-MAP.md` + per-module `CONTEXT.md` files; until then, this is the whole picture.)

## Language

**Task**:
A unit of work tracked from creation to completion — personal or work-related.
_Avoid_: To-do, item

**Person**:
An individual tracked by name and relationship (e.g., "Sarah — Manager"), reused across Tasks and Projects wherever a Requester or Recipient is needed — never typed as free text per task.
_Avoid_: Contact, user

**Requester**:
The Person who asked for a Task to be done, if anyone. Absent for self-directed/personal tasks.
_Avoid_: Assigner, assigned by

**Recipient**:
The Person expecting a Task's output back, if it owes a deliverable to someone. Independent of Requester — often the same Person, but not necessarily (a Requester can relay a request on behalf of someone else who is the actual Recipient).
_Avoid_: Deliverable to, stakeholder, owed to

**Source**:
The medium a Requester's request arrived through — email, meeting, ticket, etc. Only meaningful when a Requester is set.
_Avoid_: Channel, origin, medium

**Project**:
A tracked effort with its own description and target date, containing the set of Tasks that belong to it, viewable as a unit. A Task can belong to a Project, have a Requester, both, or neither — they're independent facts, not alternatives (a manager can assign you a piece of a larger project, giving both at once).
_Avoid_: Initiative, epic
