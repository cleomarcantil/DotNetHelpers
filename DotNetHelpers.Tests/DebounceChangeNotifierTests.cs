using DotNetHelpers.Notification;

namespace DotNetHelpers.Tests;

[TestClass]
public sealed class DebounceChangeNotifierTests
{
    [TestMethod]
    public async Task Monitore_callback_nao_deve_ser_disparado_sem_chamada_de_NotifyChanged()
    {
        DebounceChangeNotifier<string, int> notifyChanges = new(50);
        CancellationTokenSource cts = new(200);

        int changesCount = 0;
        await notifyChanges.Monitore(async changes =>
        {
            changesCount = changes.Count;
            Assert.Fail("Callback disparado!");
        }, cts.Token);

        Assert.AreEqual(0, changesCount);
    }

    [TestMethod]
    public async Task Changes_em_Monitore_callback_deve_corresponder_as_chamadas_de_NotifyChanged_com_keys_diferentes()
    {
        DebounceChangeNotifier<string, int> notifyChanges = new(50);
        CancellationTokenSource cts = new(60_000); // Ao debugar, levar em conta o timeout
        (string name, int value)[] sourceValues =
            [
                ("a", 99),
                ("b", 88),
                ("c", 77)
            ];

        foreach (var (n, v) in sourceValues)
        {
            notifyChanges.NotifyChanged(n, v);
        }

        (string name, int value)[]? changesResult = null;
        await notifyChanges.Monitore(async changes =>
        {
            changesResult = changes.ToArray();
            cts.Cancel();
        }, cts.Token);

        CollectionAssert.AreEquivalent(sourceValues, changesResult);
    }

    [TestMethod]
    public async Task Chamadas_sucessivas_de_NotifyChanged_com_mesma_key_deve_gerar_apenas_uma_change_com_ultimo_valor_em_Monitore_callback()
    {
        DebounceChangeNotifier<string, int> notifyChanges = new(50);
        CancellationTokenSource cts = new(60_000); // Ao debugar, levar em conta o timeout
        
        notifyChanges.NotifyChanged("a", 99);
        notifyChanges.NotifyChanged("a", 88);
        notifyChanges.NotifyChanged("a", 77);

        (string name, int value)[]? changesResult = null;
        await notifyChanges.Monitore(async changes =>
        {
            changesResult = changes.ToArray();
            cts.Cancel();
        }, cts.Token);

        CollectionAssert.AreEquivalent(new[] { ("a", 77) }, changesResult);
    }
}