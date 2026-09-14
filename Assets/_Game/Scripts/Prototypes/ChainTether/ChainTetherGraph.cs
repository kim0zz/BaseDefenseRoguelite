using System.Collections.Generic;
using UnityEngine;

/// <summary>Ogniwa liny: pierścień sąsiadów albo każdy do centrum.</summary>
public class ChainTetherGraph
{
    public readonly struct Link
    {
        public Link(ChainTetherBody bodyA, ChainTetherBody bodyB, int index)
        {
            BodyA = bodyA;
            BodyB = bodyB;
            Index = index;
        }

        public ChainTetherBody BodyA { get; }
        public ChainTetherBody BodyB { get; }
        public int Index { get; }
    }

    private readonly List<Link> _links = new();

    public IReadOnlyList<Link> Links => _links;

    public void Rebuild(
        ChainTetherTopology topology,
        IReadOnlyList<PlayerCharacter> players,
        ChainTetherBody hub)
    {
        _links.Clear();
        if (players == null)
            return;

        if (topology == ChainTetherTopology.Hub)
        {
            RebuildHub(players, hub);
            return;
        }

        RebuildRing(players);
    }

    private void RebuildRing(IReadOnlyList<PlayerCharacter> players)
    {
        if (players.Count < 2)
            return;

        if (players.Count == 2)
        {
            TryAdd(players[0], players[1], 0);
            return;
        }

        for (var i = 0; i < players.Count; i++)
        {
            var next = (i + 1) % players.Count;
            TryAdd(players[i], players[next], i);
        }
    }

    private void RebuildHub(IReadOnlyList<PlayerCharacter> players, ChainTetherBody hub)
    {
        if (hub == null || players.Count == 0)
            return;

        var index = 0;
        for (var i = 0; i < players.Count; i++)
        {
            var body = GetBody(players[i]);
            if (body == null)
                continue;
            _links.Add(new Link(body, hub, index));
            index++;
        }
    }

    private void TryAdd(PlayerCharacter a, PlayerCharacter b, int index)
    {
        var bodyA = GetBody(a);
        var bodyB = GetBody(b);
        if (bodyA == null || bodyB == null)
            return;
        _links.Add(new Link(bodyA, bodyB, index));
    }

    private static ChainTetherBody GetBody(PlayerCharacter player)
    {
        return player != null ? player.GetComponent<ChainTetherBody>() : null;
    }
}
