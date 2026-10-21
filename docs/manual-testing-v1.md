# Splunk Bulk Updater — Testare manuală

## Scop

Acest document conține scenariile manuale de testare pentru aplicația WPF Splunk Bulk Updater. Se folosește pentru a verifica funcționalitățile, a înregistra rezultatul fiecărei rulări și a furniza pași clari pentru tichetele Trello de bug.

Scenariile sunt reutilizabile. Nu creați câte un tichet Trello pentru fiecare scenariu. Creați un tichet de bug când rezultatul observat diferă de rezultatul așteptat.

## Cum se execută

Pentru fiecare scenariu:

1. Verificați precondițiile și notați configurația de test.
2. Executați pașii în ordine.
3. Comparați rezultatul observat cu rezultatul așteptat.
4. Marcați rezultatul: **Passed**, **Failed** sau **Blocked**.
5. Dacă eșuează, păstrați dovezile și creați un tichet de bug în Trello cu pașii exacți de reproducere.

### Înregistrarea unei rulări

Copiați acest bloc pentru fiecare rundă de testare:

- Data și ora:
- Tester:
- Versiunea / branch-ul / commit-ul aplicației:
- Versiunea Splunk și adresa instanței:
- Utilizator Splunk:
- Date/alerte folosite:
- Rezumat: Passed __ / Failed __ / Blocked __
- Linkuri către bug-uri Trello:
- Observații:

**Statusuri:**
- **Passed** — pașii și rezultatele corespund așteptărilor.
- **Failed** — cel puțin un rezultat diferă; creați un bug ticket.
- **Blocked** — scenariul nu poate fi executat din cauza unei dependențe sau probleme de mediu; notați motivul.
- **Not run** — scenariul nu a fost executat în runda curentă.

## Pregătirea mediului

- Folosiți o instanță Splunk destinată testării, nu una de producție.
- Folosiți utilizatori și alerte de test; nu modificați alerte folosite de colegi.
- Notați pentru alertele relevante: **Name, Owner, App, Sharing** și acțiunile configurate.
- Înaintea unui test de actualizare, notați sau capturați valorile inițiale.
- După actualizare, verificați valorile persistate direct în Splunk.
- Folosiți valori distinctive și ușor de identificat, de exemplu `index=e2e_old` și `index=e2e_new`.
- Nu includeți parole, tokenuri sau alte secrete în capturi, loguri ori tichete.

## Scenarii

> Pentru fiecare scenariu, înlocuiți `[ ] Not run` cu `[x] Passed`, `[x] Failed` sau `[x] Blocked`. Notați observațiile sau linkul către bug-ul Trello lângă scenariu.

### 1. Conectare și încărcarea alertelor

#### MT-01 — Conectare cu date valide

- [ ] Not run
- **Precondiții:** Instanța Splunk este pornită; utilizatorul are acces.
- **Pași:** Deschideți aplicația, introduceți datele corecte de conectare și conectați-vă.
- **Rezultat așteptat:** Aplicația se conectează și încarcă alertele vizibile utilizatorului.
- **Rezultat observat / bug:**

#### MT-02 — Conectare cu date invalide

- [ ] Not run
- **Precondiții:** Instanța Splunk este pornită.
- **Pași:** Încercați conectarea cu o parolă greșită sau cu date invalide.
- **Rezultat așteptat:** Aplicația afișează o eroare clară, nu se blochează și nu indică o conectare reușită.
- **Rezultat observat / bug:**

#### MT-03 — Instanță Splunk indisponibilă

- [ ] Not run
- **Precondiții:** Aplicația poate fi pornită; instanța este oprită sau host-ul de test este inaccesibil.
- **Pași:** Încercați să vă conectați.
- **Rezultat așteptat:** Aplicația comunică problema clar și rămâne utilizabilă după eroare.
- **Rezultat observat / bug:**

#### MT-04 — Căutare fără rezultate

- [ ] Not run
- **Precondiții:** Conectare reușită.
- **Pași:** Introduceți criterii care nu corespund niciunei alerte.
- **Rezultat așteptat:** Aplicația afișează o stare de tip „fără rezultate”; nu rămân afișate rezultate vechi ca și cum ar corespunde filtrelor.
- **Rezultat observat / bug:**

### 2. Căutare și filtre

#### MT-05 — Căutare după nume

- [ ] Not run
- **Precondiții:** Există o alertă de test cu nume cunoscut.
- **Pași:** Căutați alerta după nume.
- **Rezultat așteptat:** Este returnată alerta corectă; verificați Name, Owner și App.
- **Rezultat observat / bug:**

#### MT-06 — Filtre individuale

- [ ] Not run
- **Precondiții:** Există alerte de test cu valori cunoscute pentru App, Owner, Name, Index, SourceType și Action Type.
- **Pași:** Testați pe rând fiecare filtru: App, Owner, Name, Index, SourceType și Action Type.
- **Rezultat așteptat:** Fiecare filtru returnează doar alertele care corespund criteriului respectiv.
- **Rezultat observat / bug:**

#### MT-07 — Combinarea filtrelor

- [ ] Not run
- **Precondiții:** Există alerte care corespund unor combinații diferite de criterii.
- **Pași:** Aplicați două sau mai multe filtre în același timp.
- **Rezultat așteptat:** Rezultatele corespund tuturor filtrelor active simultan.
- **Rezultat observat / bug:**

#### MT-08 — Filtrul SPL via regex: potrivire și fără potrivire

- [ ] Not run
- **Precondiții:** Există alerte cu SPL cunoscut.
- **Pași:** Căutați un fragment/regex prezent în SPL, apoi unul care nu apare în niciun SPL.
- **Rezultat așteptat:** Prima căutare returnează potrivirile; a doua afișează starea fără rezultate.
- **Rezultat observat / bug:**

#### MT-09 — Regex invalid

- [ ] Not run
- **Precondiții:** Aplicația este conectată.
- **Pași:** Introduceți o expresie regulată invalidă în filtrul SPL.
- **Rezultat așteptat:** Aplicația indică inputul invalid și nu se blochează.
- **Rezultat observat / bug:**

#### MT-10 — Ștergerea filtrelor

- [ ] Not run
- **Precondiții:** Există filtre active și rezultate cunoscute.
- **Pași:** Ștergeți filtrele active sau folosiți opțiunea de resetare, dacă există.
- **Rezultat așteptat:** Lista revine la rezultatele corespunzătoare filtrelor rămase sau la lista nefiltrată.
- **Rezultat observat / bug:**

### 3. Selectare și preview

#### MT-11 — Selectarea unei alerte

- [ ] Not run
- **Precondiții:** Lista conține cel puțin o alertă.
- **Pași:** Selectați o alertă.
- **Rezultat așteptat:** Selecția este vizibilă și contorul de alerte selectate se actualizează corect.
- **Rezultat observat / bug:**

#### MT-12 — Selectarea mai multor alerte

- [ ] Not run
- **Precondiții:** Lista conține cel puțin două alerte.
- **Pași:** Selectați mai multe alerte și deschideți preview-ul.
- **Rezultat așteptat:** Preview-ul conține exact alertele selectate, fără alerte neselectate.
- **Rezultat observat / bug:**

#### MT-13 — Continuare fără selecție

- [ ] Not run
- **Precondiții:** Lista conține rezultate, dar nu este selectată nicio alertă.
- **Pași:** Încercați să continuați către preview sau să aplicați o modificare.
- **Rezultat așteptat:** Aplicația previne acțiunea sau explică limpede că trebuie selectată cel puțin o alertă.
- **Rezultat observat / bug:**

#### MT-14 — Închiderea preview-ului fără aplicare

- [ ] Not run
- **Precondiții:** Preview-ul afișează o modificare propusă.
- **Pași:** Închideți preview-ul fără să confirmați/aplicați modificarea.
- **Rezultat așteptat:** Nicio modificare nu este scrisă în Splunk.
- **Rezultat observat / bug:**

#### MT-15 — Verificarea modificărilor în preview

- [ ] Not run
- **Precondiții:** Există o regulă de modificare validă și alerte selectate.
- **Pași:** Deschideți preview-ul și comparați valorile inițiale cu cele propuse.
- **Rezultat așteptat:** Preview-ul identifică alertele și arată corect valorile înainte/după, înainte de confirmare.
- **Rezultat observat / bug:**

### 4. Actualizarea SPL

#### MT-16 — Înlocuire SPL pentru o alertă

- [ ] Not run
- **Precondiții:** Alertă de test cu un fragment SPL unic, de exemplu `index=e2e_old`.
- **Pași:** Selectați alerta, configurați înlocuirea cu `index=e2e_new`, verificați preview-ul și confirmați.
- **Rezultat așteptat:** Aplicația raportează rezultatul corect, iar SPL-ul salvat în Splunk conține noul fragment.
- **Rezultat observat / bug:**

#### MT-17 — Înlocuire SPL pentru mai multe alerte

- [ ] Not run
- **Precondiții:** Mai multe alerte de test conțin fragmentul vizat.
- **Pași:** Selectați alertele, configurați înlocuirea, verificați preview-ul și confirmați.
- **Rezultat așteptat:** Fiecare alertă selectată și eligibilă este actualizată corect; verificați fiecare alertă în Splunk.
- **Rezultat observat / bug:**

#### MT-18 — Înlocuire SPL fără potrivire

- [ ] Not run
- **Precondiții:** Alertă de test al cărei SPL nu conține fragmentul sursă.
- **Pași:** Aplicați o regulă validă care nu se potrivește cu SPL-ul alertei.
- **Rezultat așteptat:** Alerta nu este raportată ca modificată cu succes; rezultatul explică faptul că nu a existat potrivire.
- **Rezultat observat / bug:**

#### MT-19 — Input invalid sau obligatoriu lipsă la actualizarea SPL

- [ ] Not run
- **Precondiții:** Alertă de test selectată.
- **Pași:** Introduceți un regex invalid sau lăsați necompletat un câmp obligatoriu, apoi încercați să continuați.
- **Rezultat așteptat:** Validarea indică problema; actualizarea nu este trimisă către Splunk.
- **Rezultat observat / bug:**

#### MT-20 — Eșec la actualizare din cauza permisiunilor

- [ ] Not run
- **Precondiții:** Utilizator de test care poate vedea alerta, dar nu are permisiunea de a o modifica.
- **Pași:** Încercați o actualizare validă pentru alertă.
- **Rezultat așteptat:** Aplicația raportează eșecul pentru alerta respectivă și nu afișează un succes fals.
- **Rezultat observat / bug:**

### 5. Actualizarea acțiunilor

#### MT-21 — Actualizare câmp email

- [ ] Not run
- **Precondiții:** Alertă de test cu acțiunea email activă și un câmp cunoscut.
- **Pași:** Schimbați un singur câmp email prin funcționalitatea aplicației, verificați preview-ul și confirmați.
- **Rezultat așteptat:** Câmpul vizat persistă cu noua valoare în Splunk; celelalte setări rămân neschimbate.
- **Rezultat observat / bug:**

#### MT-22 — Actualizare câmp webhook

- [ ] Not run
- **Precondiții:** Alertă de test cu acțiunea webhook activă și un câmp cunoscut.
- **Pași:** Schimbați un singur câmp webhook, verificați preview-ul și confirmați.
- **Rezultat așteptat:** Câmpul vizat persistă cu noua valoare în Splunk; verificați alerta prin interfața Splunk sau REST API.
- **Rezultat observat / bug:**

#### MT-23 — Alertă fără acțiunea selectată

- [ ] Not run
- **Precondiții:** Alertă de test fără acțiunea email sau webhook selectată pentru actualizare.
- **Pași:** Includeți alerta într-o actualizare a tipului de acțiune respectiv.
- **Rezultat așteptat:** Aplicația indică faptul că alerta nu are acțiunea vizată sau o tratează conform comportamentului definit; nu raportează succes pentru o modificare care nu s-a produs.
- **Rezultat observat / bug:**

#### MT-24 — Actualizarea câmpurilor configurate ale acțiunii

- [ ] Not run
- **Precondiții:** Alertă cu mai multe proprietăți pentru acțiunea selectată.
- **Pași:** Configurați actualizarea unui subset de câmpuri și confirmați.
- **Rezultat așteptat:** Se schimbă doar câmpurile configurate; valorile celorlalte câmpuri rămân identice.
- **Rezultat observat / bug:**

#### MT-25 — Alertă cu mai multe acțiuni

- [ ] Not run
- **Precondiții:** Alertă de test cu mai multe acțiuni configurate, dacă mediul suportă acest caz.
- **Pași:** Modificați un singur tip de acțiune.
- **Rezultat așteptat:** Se schimbă doar acțiunea vizată; acțiunile rămase sunt păstrate.
- **Rezultat observat / bug:**

#### MT-26 — Permisiuni la actualizarea acțiunilor

- [ ] Not run
- **Precondiții:** Utilizator fără drept de modificare asupra alertei de test.
- **Pași:** Încercați să actualizați o proprietate email sau webhook.
- **Rezultat așteptat:** Aplicația raportează eșecul clar și nu indică actualizarea ca reușită.
- **Rezultat observat / bug:**

### 6. Sharing și permisiuni

#### MT-27 — Alertă privată

- [ ] Not run
- **Precondiții:** Alertă privată de test și owner-ul aferent.
- **Pași:** Conectați-vă ca owner și verificați vizibilitatea și posibilitatea de modificare.
- **Rezultat așteptat:** Owner-ul vede alerta și poate efectua acțiunile permise de configurarea Splunk.
- **Rezultat observat / bug:**

#### MT-28 — Alerte shared la nivel de app și global

- [ ] Not run
- **Precondiții:** Alerte de test cu sharing la nivel de app și global; utilizatori cu acces corespunzător.
- **Pași:** Conectați-vă cu utilizatorii de test și căutați alertele partajate.
- **Rezultat așteptat:** Vizibilitatea corespunde setării de sharing și permisiunilor utilizatorului.
- **Rezultat observat / bug:**

#### MT-29 — Utilizator care poate vedea, dar nu poate modifica alerta

- [ ] Not run
- **Precondiții:** Alertă vizibilă unui utilizator fără drept de editare.
- **Pași:** Încercați actualizarea alertei.
- **Rezultat așteptat:** Actualizarea este refuzată de Splunk și aplicația raportează eșecul fără succes fals.
- **Rezultat observat / bug:**

#### MT-30 — Identificarea alertelor cu nume similare

- [ ] Not run
- **Precondiții:** Există alerte cu același nume sau nume similar, dar cu owner/app/sharing diferite.
- **Pași:** Căutați alertele și inspectați identificarea afișată.
- **Rezultat așteptat:** Rezultatele permit diferențierea corectă prin Name, Owner, App și Sharing, acolo unde sunt afișate.
- **Rezultat observat / bug:**

### 7. Raportarea rezultatelor și anularea

#### MT-31 — Toate actualizările reușesc

- [ ] Not run
- **Precondiții:** Selecție de alerte valide, modificabile și care corespund regulii.
- **Pași:** Aplicați actualizarea și comparați raportarea aplicației cu alertele verificate în Splunk.
- **Rezultat așteptat:** Numărul și identitatea actualizărilor reușite corespund modificărilor persistate.
- **Rezultat observat / bug:**

#### MT-32 — Rezultat parțial: unele reușesc, altele eșuează

- [ ] Not run
- **Precondiții:** Selecție care include alerte modificabile și alerte nemodificabile sau neeligibile.
- **Pași:** Aplicați aceeași actualizare asupra selecției.
- **Rezultat așteptat:** Aplicația arată rezultatul fiecărei alerte și contorii de succes/eșec reflectă rezultatele reale.
- **Rezultat observat / bug:**

#### MT-33 — Reîncărcarea după actualizare

- [ ] Not run
- **Precondiții:** O alertă a fost actualizată cu succes.
- **Pași:** Reîncărcați sau căutați din nou alerta.
- **Rezultat așteptat:** Sunt afișate valorile persistate în Splunk, inclusiv modificarea aplicată.
- **Rezultat observat / bug:**

#### MT-34 — Anulare înainte de confirmare

- [ ] Not run
- **Precondiții:** Există o modificare propusă în formular sau în preview.
- **Pași:** Anulați sau închideți fluxul înainte de confirmarea finală.
- **Rezultat așteptat:** Nu se aplică nicio modificare în Splunk.
- **Rezultat observat / bug:**

## Înregistrarea unui bug în Trello

Creați un card separat pentru fiecare problemă distinctă. Titlul trebuie să rezume comportamentul observat. De exemplu: **“Bulk update reports success, but the saved search SPL remains unchanged in Splunk.”**

Folosiți formatul echipei; dacă nu există unul, includeți:

```markdown
## Summary
[Problema observată, pe scurt]

## Environment
- Application version / branch / commit:
- Splunk version:
- Splunk user:
- Alert name / owner / app:
- Sharing:
- Action type:

## Preconditions
[Starea necesară înainte de reproducere]

## Steps to reproduce
1.
2.
3.

## Expected result
[Ce trebuia să se întâmple]

## Actual result
[Ce s-a întâmplat în realitate]

## Evidence
[Screenshot, mesaj de eroare sau log relevant; eliminați secretele]

## Reproducibility
[Always / Sometimes / Once]

## Related test case
[ID-ul scenariului, de exemplu MT-16]
```

## Retest și regresie

După remedierea unui bug:

1. Reluați scenariul care a eșuat și notați rezultatul.
2. Verificați în Splunk că datele au starea așteptată.
3. Rulați scenariile apropiate care ar putea fi afectate (regression testing).
4. Actualizați statusul cardului conform procesului echipei și legați-l de scenariul testat.
