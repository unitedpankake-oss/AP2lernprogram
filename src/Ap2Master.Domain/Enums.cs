namespace Ap2Master.Domain;

public enum ExamPart
{
    Teil1 = 1,
    Teil2 = 2,
    WiSo = 3
}

public enum Difficulty
{
    Grundlagen = 1,
    Pruefung = 2,
    Anspruchsvoll = 3
}

public enum ContentType
{
    Theory = 1,
    Flashcard = 2,
    SingleChoice = 3,
    MultipleChoice = 4,
    Calculation = 5,
    Sql = 6,
    Pseudocode = 7,
    CodeReview = 8,
    BugFinding = 9,
    Uml = 10,
    ErModel = 11,
    ControlFlow = 12,
    Sorting = 13,
    Matching = 14,
    Ordering = 15,
    FillInTheBlank = 16,
    Scenario = 17,
    MockExamTask = 18,
    CheatSheet = 19
}

public enum VerificationStatus
{
    Verified = 1,
    Historical = 2,
    NeedsReviewBeforePublish = 3,
    Quarantined = 4,
    CorrectedFromSource = 5
}

public enum FlashcardRating
{
    NochEinmal = 0,
    Schwer = 3,
    Gut = 4,
    Einfach = 5
}

public enum FlashcardQueueKind
{
    Due = 1,
    Random = 2,
    Weak = 3
}

public enum MistakeType
{
    Wissensluecke = 1,
    OperatorFalschVerstanden = 2,
    Rechenfehler = 3,
    Syntaxfehler = 4,
    Modellierungsfehler = 5,
    Fluechtigkeitsfehler = 6,
    VeralteteRegelVerwechselt = 7,
    Zeitproblem = 8
}

public enum ExamKind
{
    Teil1 = 1,
    Teil2 = 2,
    WiSo = 3,
    TrainingDay = 4,
    Custom = 5
}

public enum ActivityKind
{
    Theory = 1,
    Flashcards = 2,
    Exercise = 3,
    MixedRetrieval = 4,
    WiSoSimulation = 5,
    MockTeil1 = 6,
    MockTeil2 = 7,
    WeakTopicReview = 8,
    SqlLab = 9,
    Algorithm = 10
}

public enum ThemeKind
{
    Light = 1,
    Dark = 2,
    HighContrast = 3,
    System = 4
}

public enum ConflictResolution
{
    Unresolved = 0,
    UseLegislation = 1,
    UseIhkFramework = 2,
    UseTechnicalStandard = 3,
    UseVorbereitungskurs = 4,
    UseLernzettel = 5,
    MarkHistorical = 6
}

public enum AppSection
{
    Dashboard,
    Lernplan,
    Themenkatalog,
    Lernkarten,
    Aufgabentrainer,
    SqlLabor,
    PseudocodeTrainer,
    UmlErTrainer,
    ProjektmanagementLabor,
    CoverageTrainer,
    NetzwerkSecurityTrainer,
    WiSoTrainer,
    Probeklausuren,
    Fehlerbuch,
    Statistiken,
    InhalteVerwalten,
    Einstellungen
}
