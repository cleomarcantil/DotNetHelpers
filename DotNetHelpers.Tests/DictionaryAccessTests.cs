using DotNetHelpers.Collections;

namespace DotNetHelpers.Tests;

[TestClass]
public sealed class DictionaryAccessTests
{
    [TestMethod]
    public void Count_sem_dicionario_inicializado_deve_ser_zero()
    {
        var values = new DictionaryAccess<string, object>(null);

        Assert.AreEqual(0, values.Count);
    }

    [TestMethod]
    public void Count_com_dicionario_sem_itens_deve_ser_zero()
    {
        Dictionary<string, object> sourceDic = new();
        var values = new DictionaryAccess<string, object>(sourceDic);

        Assert.AreEqual(0, values.Count);
    }

    [TestMethod]
    public void Pegar_valor_de_nome_sem_dicionario_inicializado_deve_retornar_nulo()
    {
        var values = new DictionaryAccess<string, object>(null);

        var x = values["x"];

        Assert.IsNull(x);
    }

    [TestMethod]
    public void Pegar_valor_de_nome_inexistente_de_dicionario_vazio_deve_retornar_nulo()
    {
        Dictionary<string, object> sourceDic = new();
        var values = new DictionaryAccess<string, object>(sourceDic);

        var x = values["x"];

        Assert.IsNull(x);
    }

    [TestMethod]
    public void Pegar_Count__deve_corresponder_com_quantidade_de_chaves_no_dicionário()
    {
        Dictionary<string, object> sourceDic = new()
        {
            ["x1"] = 99,
            ["x2"] = 98
        };

        var values = new DictionaryAccess<string, object>(sourceDic);

        Assert.AreEqual(2, values.Count);
    }

    [TestMethod]
    public void Pegar_valor_de_nome_deve_ser_o_mesmo_no_dicionario()
    {
        const int VALOR_TESTE = 99;
        Dictionary<string, object> sourceDic = new()
        {
            ["x"] = VALOR_TESTE,
        };

        var values = new DictionaryAccess<string, object>(sourceDic);
        var x = values["x"];

        Assert.AreEqual(VALOR_TESTE, x);
    }

    [TestMethod]
    public void Definir_valores_e_enumerar_deve_corresponder()
    {
        Dictionary<string, object> sourceDic = new()
        {
            ["x1"] = 99,
            ["x2"] = 98,
            ["x3"] = 97,
        };
        var sourceValues = sourceDic.Select(x => (x.Key, x.Value)).ToList();

        var values = new DictionaryAccess<string, object>(sourceDic);
        var result = Enumerable.ToArray(values);

        CollectionAssert.AreEquivalent(sourceValues, result);
    }
}