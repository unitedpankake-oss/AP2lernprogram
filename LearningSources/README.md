# Learning sources

Place (or keep) the two course documents here under these exact names:

- `AP2_Vorbereitungskurs_FIAE.docx`
- `AP2_Lernzettel 1.docx`

If a file arrives as `AP2_Vorbereitungskurs_FIAE (1).docx` in the repository root, copy it here and drop the `(1)`.

Then run from the repository root:

```powershell
powershell -File tools/Extract-Docx.ps1
```

The application does not embed the DOCX files as a viewer. Content is structured in `Ap2Master.Content`.
