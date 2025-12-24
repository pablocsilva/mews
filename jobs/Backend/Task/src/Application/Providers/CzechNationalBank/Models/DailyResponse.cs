using System;
using System.Collections.Generic;

namespace ExchangeRateUpdater.Providers.CzechNationalBank.Models;

/// <summary>
/// Represents the complete daily exchange rate data from CNB.
/// </summary>
/// <param name="Date">The date of the exchange rates.</param>
/// <param name="Sequence">Sequential number of the publication. Represents the number of working day according to Czech bank holidays</param>
/// <param name="Records">Collection of exchange rate records.</param>
public record DailyResponse(
    DateOnly Date,
    int Sequence,
    IReadOnlyList<DailyRecord> Records
);