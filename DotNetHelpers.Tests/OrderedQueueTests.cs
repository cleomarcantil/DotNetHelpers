using DotNetHelpers.Collections;
using System.Collections.Concurrent;

namespace DotNetHelpers.Tests;

[TestClass]
public sealed class OrderedQueueTests
{
    [TestMethod]
    [DataRow(false, DisplayName = "concurrencyLock: false")]
    [DataRow(true, DisplayName = "concurrencyLock: true")]
    public void Valores_aleatorios_devem_ser_enumerados_em_ordem(bool concurrencyLock)
    {
        int[] randomValues = [5, 2, 4, 1, 3];
        var orderedValues = randomValues.Order().ToArray();

        OrderedQueue<int> oq = new(randomValues, concurrencyLock);
        var valuesInQueue = oq.ToArray();

        CollectionAssert.AreEqual(orderedValues, valuesInQueue);
    }

    [TestMethod]
    [DataRow(false, DisplayName = "concurrencyLock: false")]
    [DataRow(true, DisplayName = "concurrencyLock: true")]
    public void Dequeue_retora_o_primeiro_da_ordem(bool concurrencyLock)
    {
        int[] randomValues = [5, 2, 4, 1, 3];
        var minValue = randomValues.Min();

        OrderedQueue<int> oq = new(randomValues, concurrencyLock);
        int? r1 = oq.TryDequeue(out var x) ? x : null;

        Assert.IsNotNull(r1);
        Assert.AreEqual(minValue, r1);
    }

    [TestMethod]
    [DataRow(false, DisplayName = "concurrencyLock: false")]
    [DataRow(true, DisplayName = "concurrencyLock: true")]
    public void Dequeue_deve_remover_da_fila(bool concurrencyLock)
    {
        int[] randomValues = [5, 2, 4, 1, 3];
        int totalCount = randomValues.Length;

        OrderedQueue<int> oq = new(randomValues, concurrencyLock);
        int count_initial = oq.Count;
        oq.TryDequeue(out var _);
        int count_less_1 = oq.Count;
        oq.TryDequeue(out var _);
        int count_less_2 = oq.Count;
        oq.TryDequeue(out var _);
        int count_less_3 = oq.Count;

        Assert.AreEqual(totalCount, count_initial);
        Assert.AreEqual(totalCount - 1, count_less_1);
        Assert.AreEqual(totalCount - 2, count_less_2);
        Assert.AreEqual(totalCount - 3, count_less_3);
    }

    [TestMethod]
    [DataRow(false, DisplayName = "concurrencyLock: false")]
    [DataRow(true, DisplayName = "concurrencyLock: true")]
    public void Apos_Dequeue_proximos_valores_devem_permanecer(bool concurrencyLock)
    {
        int[] randomValues = [5, 2, 4, 1, 3];
        var orderedValuesExceptFirst = randomValues.Order().Skip(1).ToArray();

        OrderedQueue<int> oq = new(randomValues, concurrencyLock);
        oq.TryDequeue(out var _);
        var valuesInQueue = oq.ToArray();

        CollectionAssert.AreEqual(orderedValuesExceptFirst, valuesInQueue);
    }

    [TestMethod]
    [DataRow(false, DisplayName = "concurrencyLock: false")]
    [DataRow(true, DisplayName = "concurrencyLock: true")]
    public void DequeueIf_deve_remover_o_primeiro_item_da_fila_que_atende_a_condicao(bool concurrencyLock)
    {
        int[] randomValues = [5, 2, 4, 1, 3];
        var minValue = randomValues.Min();
        var orderedValuesExceptFirst = randomValues.Order().Skip(1).ToArray();

        OrderedQueue<int> oq = new(randomValues, concurrencyLock);
        int? r1 = oq.TryDequeueIf(v => v == minValue, out var x) ? x : null;
        var valuesInQueue = oq.ToArray();

        Assert.IsNotNull(r1);
        Assert.AreEqual(minValue, r1);
        CollectionAssert.AreEqual(orderedValuesExceptFirst, valuesInQueue);
    }

    [TestMethod]
    [DataRow(false, DisplayName = "concurrencyLock: false")]
    [DataRow(true, DisplayName = "concurrencyLock: true")]
    public void DequeueIf_nao_deve_remover_o_primeiro_item_da_fila_que_nao_atende_a_condicao(bool concurrencyLock)
    {
        int[] randomValues = [5, 2, 4, 1, 3];
        var maxValue = randomValues.Max();
        var orderedValues = randomValues.Order().ToArray();

        OrderedQueue<int> oq = new(randomValues, concurrencyLock);
        int? r1 = oq.TryDequeueIf(v => v == maxValue, out var x) ? x : null;
        var valuesInQueue = oq.ToArray();

        Assert.IsNull(r1);
        CollectionAssert.AreEqual(orderedValues, valuesInQueue);
    }

    #region Concurrent tests

    [TestMethod]
    public void Adicionar_valores_paralelamente_deve_manter_a_integridade()
    {
        int[] randomValues = Enumerable.Range(1, 1000).Select(n => Random.Shared.Next(100)).ToArray();
        var orderedValues = randomValues.Order().ToArray();

        OrderedQueue<int> oq = new(concurrencyLock: true);
        Parallel.ForEach(randomValues, v => oq.Add(v));
        var valuesInQueue = oq.ToArray();

        CollectionAssert.AreEqual(orderedValues, valuesInQueue);
    }

    [TestMethod]
    [DataRow(10, DisplayName = "10 números aleatórios")]
    [DataRow(100, DisplayName = "100 números aleatórios")]
    [DataRow(1000, DisplayName = "1000 números aleatórios")]
    [DataRow(10000, DisplayName = "10000 números aleatórios")]
    public void Dequeue_paralelamente_deve_retornar_valores_correspondentes_aos_primeiros_da_ordem(int randomSize)
    {
        int dequeueCount = randomSize / 3;
        int[] randomValues = GenerateRandomValues(1, randomSize);
        var orderedValues = randomValues.Order().ToArray();
        var firstOrderedValues = orderedValues.Take(dequeueCount).ToArray();

        ConcurrentBag<int?> dequeuedValues = new();
        OrderedQueue<int> oq = new(randomValues, concurrencyLock: true);
        Parallel.For(0, dequeueCount, _ => dequeuedValues.Add(oq.TryDequeue(out var v) ? v : null));
        var orderedDequeuedValues = dequeuedValues.ToArray().Order().ToArray();

        CollectionAssert.AreEqual(firstOrderedValues, orderedDequeuedValues);
    }


    [TestMethod]
    [DataRow(10, DisplayName = "10 números aleatórios")]
    [DataRow(100, DisplayName = "100 números aleatórios")]
    [DataRow(1000, DisplayName = "1000 números aleatórios")]
    [DataRow(10000, DisplayName = "10000 números aleatórios")]
    public void Dequeue_paralelamente_deve_manter_valores_restantes_integros(int randomSize)
    {
        int dequeueCount = randomSize / 3;
        int[] randomValues = GenerateRandomValues(1, randomSize);
        var orderedValues = randomValues.Order().ToArray();
        var remainderOrderedValues = orderedValues.Skip(dequeueCount).ToArray();

        OrderedQueue<int> oq = new(randomValues, concurrencyLock: true);
        Parallel.For(0, dequeueCount, _ => oq.TryDequeue(out var _));
        var valuesInQueue = oq.ToArray();

        CollectionAssert.AreEqual(remainderOrderedValues, valuesInQueue);
    }

    #endregion

    #region Helpers

    private int[] GenerateRandomValues(int start, int end)
    {
        var numbers = Enumerable.Range(start, end).ToArray();

        Random.Shared.Shuffle(numbers);

        return numbers;
    }

    #endregion
}