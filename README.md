# Runekeep API

Backend di [Runekeep](https://github.com/AlessioBucheri/runekeep-app), in **C# / .NET 10**.

Per ora fa una cosa sola: ogni giorno scarica il price guide pubblico di Cardmarket per Riftbound e pubblica
`prices.json`, che l'app legge all'avvio (con cache offline).

**URL pubblico:** https://alessiobucheri.github.io/runekeep-api/prices.json

## Come funziona

```
Cardmarket (file pubblici, game id 22)          GitHub Actions, ogni giorno alle 03:41 UTC
  price_guide_22.json  ─┐
                        ├─► Runekeep.PriceJob ─► site/prices.json ─► GitHub Pages ─► app
  products_singles_22.json ┘
```

- Nessun server da tenere acceso e nessun costo: GitHub Actions è gratuito per i repository pubblici.
- Se Cardmarket non risponde, il job fallisce senza pubblicare: resta online il file del giorno prima.

## Progetti

| Progetto                   | Ruolo                                                                    |
| -------------------------- | ------------------------------------------------------------------------ |
| `src/Runekeep.Prices`      | Libreria: client dei file Cardmarket e trasformazione in snapshot (pura) |
| `src/Runekeep.PriceJob`    | Console: scarica, trasforma, scrive `prices.json`                        |
| `tests/Runekeep.Prices.Tests` | Test xUnit con copie ridotte dei file reali                           |

La logica sta tutta nella libreria: quando servirà un'API ASP.NET vera (account, sincronizzazione), la userà
così com'è.

## Formato di `prices.json`

```json
{
  "version": 1,
  "source": "cardmarket",
  "priceDate": "2026-10-09T02:45:56+02:00",
  "generatedAt": "2026-10-09T03:41:12+00:00",
  "count": 1234,
  "prices": { "845712": { "avg": 0.04, "trend": 0.04, "low": 0.02, "foil": 0.17 } }
}
```

Le chiavi di `prices` sono gli `idProduct` di Cardmarket, che sono anche gli id carta nell'app.
`avg` è la media degli ultimi 30 giorni (o la media generale se manca), `foil` la media 30 giorni del foil;
`0` significa nessun dato.

## Comandi

```bash
dotnet build
dotnet test
dotnet run --project src/Runekeep.PriceJob -- site   # scrive site/prices.json
```

Il job si può lanciare a mano da GitHub: _Actions → Prezzi giornalieri → Run workflow_.

## Da sapere

GitHub sospende i workflow pianificati dei repository pubblici dopo 60 giorni senza attività nel repo e
manda un'email: basta riattivarlo da _Actions_ (o fare un commit).
