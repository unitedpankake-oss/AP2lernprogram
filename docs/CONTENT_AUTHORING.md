# Content authoring

JSON/SQL/C# catalog is the source of truth, not XAML.

Required fields on `LearningContent`: Id, Title, TopicId, ExamPart, Difficulty, ContentType, tags, time, points, Q/A/explanation/hints, source attribution, validity, verification, version.

Validation rejects duplicate ids, missing answers, bad topic refs, missing sources, expired non-historical facts, ambiguous MC, bad formula names, quarantined conflicts.

AI drafts must not enter `IsApprovedForExam` until structural, answer, duplicate and source checks plus manual approval (`ContentApproval`).
