# Random generators

`SeededTemplateEngine` uses `Random(seed)`. The same template+seed always yields the same task text, solution and parameter JSON (tested).

Placeholders `{{name}}` resolve from a JSON schema (`int`, `amount`, `pick`, `name`, `company`, `date`, `ipv4`, `array`). Derived values:

- Wirtschaftlichkeit = Ertrag/Aufwand
- Rentabilität = Gewinn/Kapital×100
- Amtsgericht vs Landgericht at 10.000 €
- Netzplan duration = A + max(B,C) for the three-activity template

Impossible ranges (`min > max`, zero Aufwand) throw before presentation.
