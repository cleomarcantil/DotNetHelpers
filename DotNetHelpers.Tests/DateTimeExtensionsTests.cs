using DotNetHelpers.Extensions;
using System.Globalization;

namespace DotNetHelpers.Tests;

[TestClass]
public sealed class DateTimeExtensionsTests
{
    [TestMethod]
    [DataRow(["05/10/2024", 7, "07/10/2024"])]
    [DataRow(["05/10/2024", 4, "04/11/2024"])]
    [DataRow(["05/11/2024", 30, "30/11/2024"])]
    [DataRow(["05/11/2024", 31, "31/12/2024"])]
    [DataRow(["01/01/2025", 29, "29/01/2025"])]
    [DataRow(["01/02/2025", 29, "29/03/2025"])]
    public void Encontrar_proximo_dia_X_a_parir_de_uma_data(string sdata, int nextSpecificDay, string sexpectedDate)
    {
        var dataBase = DateTime.ParseExact(sdata, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var expectedDate = DateTime.ParseExact(sexpectedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

        var nextDateForDay = dataBase.Find(day: nextSpecificDay);

        Assert.AreEqual(expectedDate, nextDateForDay);
    }

    [TestMethod]
    [DataRow(["05/10/2024", -7, "07/09/2024"])]
    [DataRow(["05/10/2024", -4, "04/10/2024"])]
    [DataRow(["05/11/2024", -30, "30/10/2024"])]
    [DataRow(["05/11/2024", -31, "31/10/2024"])]
    [DataRow(["01/01/2025", -29, "29/12/2024"])]
    [DataRow(["01/03/2025", -29, "29/01/2025"])]
    public void Encontrar_dia_X_anterior_a_parir_de_uma_data(string sdata, int nextSpecificDay, string sexpectedDate)
    {
        var dataBase = DateTime.ParseExact(sdata, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var expectedDate = DateTime.ParseExact(sexpectedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

        var prevDateForDay = dataBase.Find(day: nextSpecificDay);

        Assert.AreEqual(expectedDate, prevDateForDay);
    }

    [TestMethod]
    [DataRow(["05/10/2024", 7, "05/10/2024"])]
    [DataRow(["05/10/2024", 6, "11/10/2024"])]
    [DataRow(["08/10/2024", 4, "09/10/2024"])]
    [DataRow(["27/10/2024", 7, "02/11/2024"])]
    public void Encontrar_proximo_dia_da_semana_a_parir_de_uma_data(string sdata, int nextSpecificWeekDay, string sexpectedDate)
    {
        var dataBase = DateTime.ParseExact(sdata, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var expectedDate = DateTime.ParseExact(sexpectedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

        var nextDateForWeekDay = dataBase.Find(weekDay: nextSpecificWeekDay);

        Assert.AreEqual(expectedDate, nextDateForWeekDay);
    }

    [TestMethod]
    [DataRow(["05/10/2024", -7, "05/10/2024"])]
    [DataRow(["05/10/2024", -6, "04/10/2024"])]
    [DataRow(["08/10/2024", -4, "02/10/2024"])]
    [DataRow(["06/11/2024", -7, "02/11/2024"])]
    public void Encontrar_dia_da_semana_anteior_a_parir_de_uma_data(string sdata, int nextSpecificWeekDay, string sexpectedDate)
    {
        var dataBase = DateTime.ParseExact(sdata, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var expectedDate = DateTime.ParseExact(sexpectedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

        var prevDateForWeekDay = dataBase.Find(weekDay: nextSpecificWeekDay);

        Assert.AreEqual(expectedDate, prevDateForWeekDay);
    }

    [TestMethod]
    [DataRow(["05/10/2024", 5, 7, "05/10/2024"])]
    [DataRow(["05/10/2024", 8, 6, "08/11/2024"])]
    [DataRow(["08/10/2024", 1, 1, "01/12/2024"])]
    [DataRow(["27/10/2024", 29, 7, "29/03/2025"])]
    public void Encontrar_proximos_dia_e_dia_da_semana_a_parir_de_uma_data(string sdata, int nextSpecificDay, int nextSpecificWeekDay, string sexpectedDate)
    {
        var dataBase = DateTime.ParseExact(sdata, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var expectedDate = DateTime.ParseExact(sexpectedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

        var nextDateForDayAndWeekDay = dataBase.Find(day: nextSpecificDay, weekDay: nextSpecificWeekDay);

        Assert.AreEqual(expectedDate, nextDateForDayAndWeekDay);
    }

    [TestMethod]
    [DataRow(["05/10/2024", -5, -7, "05/10/2024"])]
    [DataRow(["05/10/2024", -8, -6, "08/03/2024"])]
    [DataRow(["08/10/2024", -1, -1, "01/09/2024"])]
    [DataRow(["27/10/2024", -17, -7, "17/08/2024"])]
    public void Encontrar_dia_e_dia_da_semana_anteriores_a_parir_de_uma_data(string sdata, int nextSpecificDay, int nextSpecificWeekDay, string sexpectedDate)
    {
        var dataBase = DateTime.ParseExact(sdata, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var expectedDate = DateTime.ParseExact(sexpectedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

        var nextDateForDayAndWeekDay = dataBase.Find(day: nextSpecificDay, weekDay: nextSpecificWeekDay);

        Assert.AreEqual(expectedDate, nextDateForDayAndWeekDay);
    }


}
