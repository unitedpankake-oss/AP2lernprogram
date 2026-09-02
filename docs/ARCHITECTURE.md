# Architecture

## Layers

- **Ap2Master.Domain** – entities, SM-2, readiness weights, coverage, network plan, SQL comparison, content rules
- **Ap2Master.Application** – ports, learning plan, catalog validation
- **Ap2Master.Generators** – seeded templates, IHK-style pseudocode interpreter
- **Ap2Master.ExamEngine** – scoring, WiSo integrity, coverage/network facades
- **Ap2Master.Content** – normalized topic tree, teaching facts, WiSo bank, Helpdesk schema, conflicts
- **Ap2Master.Infrastructure** – EF Core SQLite, seed, backup/restore, SQL lab, exam persistence
- **Ap2Master.App** – WPF shell, Fluent themes, German UI, no business logic in code-behind except editor/nav glue

## Data

SQLite file `%LocalAppData%\Ap2Master\ap2master.db` is created and seeded on first start (`EnsureCreated` + catalog import). Learner progress (reviews, answers, exam sessions, mistakes) is stored there and survives restart.

## AI

`ILearningContentAiProvider` remains a null implementation for *catalog drafts*. The **AP2 KI-Assistent** is a separate local module (`IAiAssistantService`) with FTS5 retrieval, exam guards, and optional localhost Ollama. See `docs/AI_ASSISTANT.md`. Secrets use DPAPI (`ISecureSecretStore`), never source control.

## Theming

Light / dark / high-contrast resource brushes, font scale, reduced-motion flag, focus borders, text+icon navigation.
