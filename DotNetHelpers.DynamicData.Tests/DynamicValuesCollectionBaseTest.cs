
namespace DotNetHelpers.DynamicData.Tests;

[TestClass]
public sealed class DynamicValuesCollectionBaseTest
{
    [TestMethod]
    public void Count_sem_dicionario_inicializado_deve_ser_zero()
    {
        var values = new DynamicValuesCollection(null, false);

        Assert.AreEqual(0, values.Count);
    }

    [TestMethod]
    public void Count_com_dicionario_sem_itens_deve_ser_zero()
    {
        var values = new DynamicValuesCollection(new(), false);

        Assert.AreEqual(0, values.Count);
    }
    
    [TestMethod]
    public void Pegar_valor_de_nome_inexistente_deve_retornar_nulo()
    {
        var values = new DynamicValuesCollection(new(), false);

        var x = values["x"];

        Assert.IsNull(x);
    }
    
    [TestMethod]
    public void Definir_valores_e_pegar_Count_deve_corresponder()
    {
        var values = new DynamicValuesCollection(new(), false);

        values["x1"] = 99;
        values["x2"] = 98;

        Assert.AreEqual(2, values.Count);
    }
    
    [TestMethod]
    public void Definir_novo_valor_e_pegar_de_volta_deve_ser_o_mesmo()
    {
        const int VALOR_TESTE = 99;
        var values = new DynamicValuesCollection(new(), false);

        values["x"] = VALOR_TESTE;
        var x = values["x"];

        Assert.AreEqual(VALOR_TESTE, x);
    }    
    
    [TestMethod]
    public void Definir_valor_nulo_e_pegar_de_volta_deve_ser_nulo()
    {
        var values = new DynamicValuesCollection(new(), false);

        values["x"] = null;
        var x = values["x"];

        Assert.IsNull(x);
    }    
    
    [TestMethod]
    public void Atualizar_valor_com_nulo_deve_remover_decrementando_Count()
    {
        var values = new DynamicValuesCollection(new(), false);

        values["x"] = 51;
        values["y"] = 52;
        values["z"] = 52;

        Assert.AreEqual(3, values.Count);
        values["x"] = null;
        Assert.AreEqual(2, values.Count);
    }
    
    [TestMethod]
    public void Atualizar_valor_existente_com_nulo_e_pegar_de_volta_deve_ser_o_mesmo()
    {
        var values = new DynamicValuesCollection(new(), false);

        values["x"] = 55;
        values["x"] = null;
        var x = values["x"];

        Assert.IsNull(x);
    }
    
    [TestMethod]
    public void Definir_valores_e_enumerar_deve_corresponder()
    {
        (string name, object value)[] testValues =
            [
                ("x1", 99),
                ("x2", 98),
                ("x3", 97),
            ];

        var values = new DynamicValuesCollection(new(), false);
        foreach (var (name, value) in testValues )
        {
            values[name] = value;
        }
        var result = Enumerable.ToArray(values);

        CollectionAssert.AreEquivalent(testValues, result);
    }
    
    class DynamicValuesCollection(Dictionary<string, object>? dictionary, bool keepNulls) 
        : DynamicValuesCollectionBase<object>(keepNulls)
    {
        protected override IDictionary<string, object>? GetSourceDictionary()
            => dictionary;

        public new object? this[string name]
        {
            get => base[name];
            set => base[name] = value;
        }
    }
}