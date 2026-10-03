using System;
using System.Globalization;

namespace Jellyfin.Plugin.Dlna.Didl;

/// <summary>
/// Resolves and formats dates for DLNA DIDL metadata (live TV EPG, channels).
/// </summary>
public static class DlnaItemDisplayDate
{
    /// <summary>
    /// Returns whether a date is safe to expose to DLNA clients (avoids Unix epoch display).
    /// </summary>
    /// <param name="date">The date to validate.</param>
    /// <returns>True if the date is valid for DLNA metadata; otherwise false.</returns>
    public static bool IsValidDlnaDate(DateTime date)
    {
        if (date == default || date.Year < 1980)
        {
            return false;
        }

        return !(date.Year == 1970 && date.Month == 1 && date.Day <= 2);
    }

    /// <summary>
    /// Formats a UTC instant for Dublin Core / UPnP datetime fields (local wall time).
    /// </summary>
    /// <param name="utcDate">The UTC (or unspecified) date to format.</param>
    /// <param name="timeZone">Optional time zone; defaults to local.</param>
    /// <returns>A formatted local datetime string suitable for DIDL.</returns>
    public static string FormatDlnaDateTime(DateTime utcDate, TimeZoneInfo? timeZone = null)
    {
        var zone = timeZone ?? TimeZoneInfo.Local;
        var local = utcDate.Kind == DateTimeKind.Utc
            ? TimeZoneInfo.ConvertTimeFromUtc(utcDate, zone)
            : TimeZoneInfo.ConvertTime(utcDate, zone);
        return local.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Display date for a <see cref="MediaBrowser.Controller.LiveTv.LiveTvProgram"/> (air start).
    /// </summary>
    /// <param name="startDate">The programme start date.</param>
    /// <returns>The start date when valid; otherwise null.</returns>
    public static DateTime? GetProgramDisplayDateUtc(DateTime startDate)
        => IsValidDlnaDate(startDate) ? startDate : null;

    /// <summary>
    /// Display date for a live TV channel (current programme start, or <paramref name="utcNow"/> if no EPG).
    /// </summary>
    /// <param name="currentProgramStartUtc">The current EPG programme start, if any.</param>
    /// <param name="utcNow">Fallback UTC timestamp when no valid EPG start exists.</param>
    /// <returns>The channel display date in UTC.</returns>
    public static DateTime? GetChannelDisplayDateUtc(DateTime? currentProgramStartUtc, DateTime utcNow)
    {
        if (currentProgramStartUtc.HasValue && IsValidDlnaDate(currentProgramStartUtc.Value))
        {
            return currentProgramStartUtc.Value;
        }

        return utcNow;
    }

    /// <summary>
    /// Display date from library premiere metadata when valid.
    /// </summary>
    /// <param name="premiereDate">The item premiere date, if any.</param>
    /// <returns>The premiere date when valid; otherwise null.</returns>
    public static DateTime? GetPremiereDisplayDateUtc(DateTime? premiereDate)
        => premiereDate.HasValue && IsValidDlnaDate(premiereDate.Value) ? premiereDate.Value : null;
}
