using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Ray
{
    public int X1 { get; private set; }
    public int Y1 { get; private set; }
    public int X2 { get; private set; }
    public int Y2 { get; private set; }
    public Location A
    {
        get
        {
            return new Location(X1, Y1);
        }
    }
    public Location B
    {
        get
        {
            return new Location(X2, Y2);
        }
    }

    public Ray(int x1, int y1, int x2, int y2)
    {
        this.X1 = x1;
        this.Y1 = y1;
        this.X2 = x2;
        this.Y2 = y2;
    }

    public Ray(Location c1, Location c2)
    {
        this.X1 = c1.X;
        this.Y1 = c1.Y;
        this.X2 = c2.X;
        this.Y2 = c2.Y;
    }

    public string Loggable() => $"a: [{X1}|{Y1}], b: [{X2}|{Y2}]";

    public void Iterate(int x, int y)
    {
        X1 = X2;
        Y1 = Y2;
        X2 = x;
        Y2 = y;
    }

    public void Iterate(Location next)
    {
        X1 = X2;
        Y1 = Y2;
        X2 = next.X;
        Y2 = next.Y;
    }

    public Ray Rotated(double angle)
    {
        double cos = Math.Cos(angle);
        double sin = Math.Sin(angle);

        int x = (int)(cos * (X2 - X1) - sin * (Y2 - Y1) + X1);
        int y = (int)(sin * (X2 - X1) + cos * (Y2 - Y1) + Y1);

        return new Ray(X1, Y1, x, y);
    }

    public Ray Reversed()
    {
        // Swaps the starting point and ending point
        return new Ray(X2, Y2, X1, Y1);
    }

    public Ray Opposite()
    {
        // Keeps the original starting point, but places the ending point in the opposite direction
        return new Ray(X2, Y2, X1 + (X1 - X2), Y1 + (Y1 - Y2));
    }

    public double AngleBetween(Ray other)
    {
        // Copilot suggested this version. Works as good as other, but there is a tiny margin of error between them.
        double dot = (X2 - X1) * (other.X2 - other.X1) + (Y2 - Y1) * (other.Y2 - other.Y1);
        double det = (X2 - X1) * (other.Y2 - other.Y1) - (Y2 - Y1) * (other.X2 - other.X1);
        return Math.Atan2(det, dot);
    }
}

class Location
{
    public int X { get; }
    public int Y { get; }

    public Location(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Location(Location other)
    {
        X = other.X;
        Y = other.Y;
    }

    public double Proximity(Location other)
    {
        int deltaX = X - other.X;
        int deltaY = Y - other.Y;
        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    public Location Midpoint(Location other)
    {
        int x = (X + other.X) / 2;
        int y = (Y + other.Y) / 2;
        return new Location(x, y);
    }

    public List<Location> SortedByProximity(List<Location> others)
    {
        return others.OrderBy(o => Proximity(o)).ToList();
    }

    public static Location Average(List<Location> locations)
    {
        int x = (int)locations.Average(l => l.X);
        int y = (int)locations.Average(l => l.Y);
        return new Location(x, y);
    }
}

class Unit
{
    public int Id { get; set; }
    public int Owner { get; set; }
    public Location Location { get; set; }
    public int Type { get; set; }
    public int Health { get; set; }

    public Unit(int id, int owner, Location location, int type, int health)
    {
        Id = id;
        Owner = owner;
        Location = location;
        Type = type;
        Health = health;
    }
}

class UnitTracker
{
    private Dictionary<int, Unit> Units;

    public UnitTracker(List<Unit> units)
    {
        Units = new Dictionary<int, Unit>();
        foreach (var unit in units)
        {
            Add(unit);
        }
    }

    public Unit Get(int id)
    {
        return Units[id];
    }

    private void Add(Unit unit)
    {
        if (unit.Type == 0)
        {
            Units.Add(
                unit.Id,
                new Knight(unit.Id, unit.Owner, unit.Location, unit.Type, unit.Health)
            );
        }
        else
        {
            Units.Add(unit.Id, unit);
        }
    }

    public List<Unit> AllUnits()
    {
        return Units.Values.ToList();
    }

    public List<Unit> UnitsOwnedBy(int owner)
    {
        return Units.Values.Where(u => u.Owner == owner).ToList();
    }

    public List<Unit> EnemyUnits()
    {
        return UnitsOwnedBy(1);
    }

    public List<Unit> FriendlyUnits()
    {
        return UnitsOwnedBy(0);
    }

    public List<Unit> UnitsByProximity(Location location)
    {
        return Units.Values.OrderBy(u => u.Location.Proximity(location)).ToList();
    }

    public static List<Unit> UnitsByProximityTo(List<Unit> units, Location location)
    {
        return units.OrderBy(u => u.Location.Proximity(location)).ToList();
    }

    // public static List<Knight> UnitsByProximityTo(List<Knight> units, Location location) {
    //     return UnitsByProximityTo
    //     return units.OrderBy(u => u.Location.Proximity(location)).ToList();
    // }
}

class Knight : Unit
{
    public Knight(int id, int owner, Location location, int type, int health)
        : base(id, owner, location, type, health)
    {
    }
}

class QueenUpdate
{
    public Location location { get; set; }
    public int health { get; set; }
    public int touchedSite { get; set; }

    public QueenUpdate(Location location, int health, int touchedSite)
    {
        this.location = location;
        this.health = health;
        this.touchedSite = touchedSite;
    }
}

class Queen : Unit
{
    public int TouchedSite;
    private QueenBrain brain;
    public QueenSenses Senses;

    public Queen(int id, int owner, Location location, int type, int health)
        : base(id, owner, location, type, health)
    {
        this.TouchedSite = -1;
        brain = new QueenBrain(this);
    }

    public void TakeTurn(GameState state)
    {
        Senses = new QueenSenses(this, state);
        brain.Think(state);
    }

    public void Update(QueenUpdate update, GameState state)
    {
        Location = update.location;
        Health = update.health;
        TouchedSite = update.touchedSite;
    }

    public void Move(Location target)
    {
        Console.WriteLine($"MOVE {target.X} {target.Y}");
    }

    public void BuildMine(int siteId)
    {
        Console.WriteLine($"BUILD {siteId} MINE");
    }

    public void BuildTower(int siteId)
    {
        Console.WriteLine($"BUILD {siteId} TOWER");
    }

    public void BuildBarracks(int siteId, string barracksType)
    {
        Console.WriteLine($"BUILD {siteId} BARRACKS-{barracksType}");
    }


    public void Wait()
    {
        Console.WriteLine("WAIT");
    }

    public bool IsTouchingSite()
    {
        return TouchedSite != -1;
    }

    public Site GetTouchedSite(SiteTracker tracker)
    {
        if (IsTouchingSite())
        {
            return tracker.Get(TouchedSite);
        }
        else
        {
            return null;
        }
    }

    // Could include option for attackerCount
    public bool IsUnderAttack(int proximityThreshold)
    {
        var nearestKnight = Senses.NearestEnemyKnight();
        if (nearestKnight == null)
        {
            return false;
        }
        return Location.Proximity(nearestKnight.Location) < proximityThreshold;
    }
}

class QueenSenses
{
    private Queen queen;
    private GameState state;

    public QueenSenses(Queen queen, GameState state)
    {
        this.queen = queen;
        this.state = state;
    }

    public Knight NearestEnemyKnight()
    {
        return UnitTracker.UnitsByProximityTo(state.UnitTracker.EnemyUnits(), queen.Location).OfType<Knight>().FirstOrDefault();
    }

    public Location AverageEnemyKnightLocationWithin(int proximityThreshold)
    {
        var knights = state.UnitTracker.EnemyUnits().OfType<Knight>().Where(k => k.Location.Proximity(queen.Location) < proximityThreshold);
        var averageLocation = Location.Average(knights.Select(k => k.Location).ToList());
        return averageLocation;
    }

    public Ray AwayFromNearbyHordes(int proximityThreshold)
    {
        var averageLocation = AverageEnemyKnightLocationWithin(proximityThreshold);
        return new Ray(queen.Location, averageLocation).Opposite();
    }

    public Ray NearestEnemyKnightDirection()
    {
        var knight = NearestEnemyKnight();
        return new Ray(queen.Location, knight.Location);
    }

     public Site NearestSafeBuildSite()
    {
        var safeSites = state.SiteTracker.SafeBuildSites();
        var sortedSites = SiteTracker.SitesByProximityTo(safeSites, queen.Location);
        return sortedSites.FirstOrDefault();
    }
}

class QueenBrain
{

    private Queen queen;
    private Strategy currentStrategy;

    public QueenBrain(Queen queen)
    {
        this.queen = queen;
        this.currentStrategy = new ExploreStrategy(queen);
    }

    public void Think(GameState state)
    {
        currentStrategy = currentStrategy.GetNextStrategy(state);

        currentStrategy.Execute(state);
    }
}

interface Strategy
{
    Strategy GetNextStrategy(GameState state);
    void Execute(GameState state);
}

class ExploreStrategy : Strategy
{
    private Queen queen;

    public ExploreStrategy(Queen queen)
    {
        this.queen = queen;
    }

    public Strategy GetNextStrategy(GameState state)
    {
        var site = queen.GetTouchedSite(state.SiteTracker);
        if (queen.IsUnderAttack(190))
        {
            return new FleeStrategy(queen);
        } else if (queen.IsTouchingSite())
        {
            if(site.IsTower() & site.IsFriendly() & site.Param1 < site.Param2) {
                return new BuildTowerStrategy(queen, site);
            } else if (site.IsGoldMine() & site.IsFriendly() & site.Param1 < site.MaxMineSize & site.GoldRemaining > 80) {
                return new BuildMineStrategy(queen, site);
            // } else if (queen.IsNearEnemyTower()) {
            //     return new AvoidTowerStrategy(queen);
            } else if (!state.ShouldSave & state.SiteTracker.OwnedBarracks().Count() < 1) {
                return new BuildBarracksStrategy(queen, site);
            } else if (!site.IsFriendly()) {
                return new CaptureSiteStrategy(queen, site);
            } else {
                return this;
            }

        } else {
            return this;
        }
    }

    public void Execute(GameState state)
    {
        Console.Error.WriteLine("Exploring");
        var touchedSite = queen.GetTouchedSite(state.SiteTracker);
        if (touchedSite != null) {
            if (!touchedSite.IsFriendly()) {
                if(queen.IsUnderAttack(400)) {
                    queen.BuildTower(touchedSite.Id);
                } else {
                    queen.BuildMine(touchedSite.Id);
                }
            } else {
                approachNearbyBuildingSite();
            }
        } else {
            approachNearbyBuildingSite();
        }
    }

    private void approachNearbyBuildingSite()
    {
        var viableSite = queen.Senses.NearestSafeBuildSite();
        queen.Move(viableSite.Location);
    }
}

class FleeStrategy : Strategy
{
    private Queen queen;

    public FleeStrategy(Queen queen)
    {
        this.queen = queen;
    }

    public Strategy GetNextStrategy(GameState state)
    {
        if (queen.IsUnderAttack(120))
        {
            return new FleeStrategy(queen);
        }
        return new ExploreStrategy(queen);
    }

    public void Execute(GameState state)
    {
        var site = queen.GetTouchedSite(state.SiteTracker);
        var siteExists = site != null;
        if (siteExists) {

            if (queen.IsTouchingSite() & site.IsFriendly() & site.IsGoldMine() & site.GoldRemaining < 100) {
                queen.BuildTower(site.Id);
            } else if (queen.IsTouchingSite() & !site.IsTower() & !site.IsGoldMine()) {
                queen.BuildTower(site.Id);
            } else if (queen.IsTouchingSite() & !site.IsFriendly()) {
                queen.BuildTower(site.Id);
        }
        } else {
           
            var awayFromHorde = queen.Senses.AwayFromNearbyHordes(200);
            queen.Move(awayFromHorde.B);
        }
    }
}

class BuildTowerStrategy : Strategy
{
    private Queen queen;
    private Site site;

    public BuildTowerStrategy(Queen queen, Site site)
    {
        this.queen = queen;
        this.site = site;
    }

    public Strategy GetNextStrategy(GameState state)
    {
        return new ExploreStrategy(queen);
    }

    public void Execute(GameState state)
    {
        queen.BuildTower(site.Id);
    }
}

class BuildMineStrategy : Strategy {
    private Queen queen;
    private Site mine;

    public BuildMineStrategy(Queen queen, Site mine) {
        this.queen = queen;
        this.mine = mine;
    }

    public Strategy GetNextStrategy(GameState state) {
        return new ExploreStrategy(queen);
    }

    public void Execute(GameState state) {
        var touchedSite = queen.GetTouchedSite(state.SiteTracker);
        if (touchedSite.IsGoldMine() & touchedSite.IsFriendly() & touchedSite.Param1 < touchedSite.MaxMineSize & touchedSite.GoldRemaining > 80) {
            queen.BuildMine(touchedSite.Id);
        }
    }
}

class BuildBarracksStrategy : Strategy {
    private Queen queen;
    private Site site;
    public BuildBarracksStrategy(Queen queen, Site site) {
        this.queen = queen;
        this.site = site;
    }

    public Strategy GetNextStrategy(GameState state) {
        return new ExploreStrategy(queen);
    }

    public void Execute(GameState state) {
        queen.BuildBarracks(site.Id, "KNIGHT");
    }
}

class CaptureSiteStrategy : Strategy {
    private Queen queen;
    private Site site;
    public CaptureSiteStrategy(Queen queen, Site site) {
        this.queen = queen;
        this.site = site;
    }

    public Strategy GetNextStrategy(GameState state) {
        return new ExploreStrategy(queen);
    }

    public void Execute(GameState state) {
        if (queen.IsUnderAttack(400)) {
            queen.BuildTower(site.Id);
        } else {
            queen.BuildMine(site.Id);
        }
    }
}


class QueenBrainV1
{

    // ideas
    // Build/Train giants if they have lots of towers
    // Running away too much allows for their faster expansion
    // If near an enemy structure queen can destroy, prioritize that
    private Queen queen;
    private GameState state;

    public QueenBrainV1(Queen queen)
    {
        this.queen = queen;
        this.state = new GameState();
    }

    public void Think(GameState state)
    {
        this.state = state;
        var touchedSite = queen.GetTouchedSite(state.SiteTracker);

        if (IsNearEnemyTower())
        {
            Console.Error.WriteLine("I'm near enemy tower!");
            var enemyTowerDirection = GetDirectionToNearestEnemyTower();
            var awayFromTower = enemyTowerDirection.Opposite();
            queen.Move(awayFromTower.B);
        }
        else if (IsUnderAttack(170))
        {
            Console.Error.WriteLine("I'm under seige");
            var enemyKnightDirection = GetDirectionToNearestEnemyKnight();
            var awayFromKnight = enemyKnightDirection.Opposite();
            if (queen.IsTouchingSite())
            {
                if (!touchedSite.IsTower())
                {
                    queen.BuildTower(touchedSite.Id);
                }
                else
                {
                    queen.Move(awayFromKnight.B);
                }
            }
            else
            {
                queen.Move(awayFromKnight.B);
            }
        }
        else if (queen.IsTouchingSite())
        {
            // Expand if queen just built a tower or mine
            if (touchedSite.IsTower() & touchedSite.IsFriendly() & touchedSite.Param1 < touchedSite.Param2)
            {
                Console.Error.WriteLine("expanding tower");
                // expand the tower
                queen.BuildTower(touchedSite.Id);
            }
            else if (touchedSite.IsGoldMine() & touchedSite.IsFriendly() & touchedSite.Param1 < touchedSite.MaxMineSize & touchedSite.GoldRemaining > 80)
            {
                // expand the mine
                Console.Error.WriteLine("expanding mine");
                queen.BuildMine(touchedSite.Id);
            }
            else
            {
                if (IsUnderAttack(385) & !touchedSite.IsTower())
                {
                    // Build tower if in danger
                    Console.Error.WriteLine("building tower");
                    queen.BuildTower(touchedSite.Id);
                }
                else if (!state.ShouldSave & state.SiteTracker.OwnedBarracks().Count() < 1)
                {
                    // Build barracks if we need one
                    queen.BuildBarracks(touchedSite.Id, "KNIGHT");
                }
                else if (!touchedSite.IsFriendly())
                {
                    if (touchedSite.GoldRemaining > 50)
                    {
                        Console.Error.WriteLine("building mine");
                        queen.BuildMine(touchedSite.Id);
                    }
                    else
                    {
                        // just more towers then
                        queen.BuildTower(touchedSite.Id);
                    }
                }
                else
                {
                    // Get a move on if we're not doing anything
                    queen.Move(NearestSafeBuildSite().Location);
                }
            }
        }
        else
        {
            // if the queen is not near a building site, move to it
            var nearestSite = NearestSafeBuildSite();
            if (nearestSite != null)
            {
                queen.Move(nearestSite.Location);
            }
            else
            {
                queen.Wait();
            }
        }
    }

    private Site NearestSafeBuildSite()
    {
        var safeSites = state.SiteTracker.SafeBuildSites();
        var sortedSites = SiteTracker.SitesByProximityTo(safeSites, queen.Location);
        return sortedSites.FirstOrDefault();
    }

    private Site NearestSite()
    {
        var sortedSites = state.SiteTracker.SitesByProximity(queen.Location);
        return sortedSites.FirstOrDefault();
    }

    private List<Unit> FriendlyUnits()
    {
        var friendlyUnits = state.UnitTracker.FriendlyUnits();
        return friendlyUnits;
    }

    private Knight NearestEnemyKnight()
    {
        var sortedKnights = state.UnitTracker.UnitsByProximity(queen.Location).OfType<Knight>().ToList();
        if (sortedKnights.Count < 1)
        {
            return null;
        }
        return sortedKnights[0];
    }

    private bool IsUnderAttack(int proximityThreshold)
    {
        var nearestKnight = NearestEnemyKnight();
        if (nearestKnight == null)
        {
            return false;
        }
        return queen.Location.Proximity(nearestKnight.Location) < proximityThreshold;
    }

    private Ray GetDirectionToNearestEnemyKnight()
    {
        var nearestKnight = NearestEnemyKnight();
        if (nearestKnight == null)
        {
            throw new Exception("No knights found, cannot get directions");
        }
        return new Ray(queen.Location, nearestKnight.Location);
    }

    public Tower NearestEnemyTower()
    {
        var HostileTowers = state.SiteTracker.HostileTowers();
        var sortedTowers = SiteTracker.SitesByProximityTo(HostileTowers, queen.Location);
        return sortedTowers.FirstOrDefault();
    }

    private Ray GetDirectionToNearestEnemyTower()
    {
        var enemyTower = NearestEnemyTower();
        if (enemyTower == null)
        {
            throw new Exception("No enemy towers found, cannot get directions.");
        }
        return new Ray(queen.Location, enemyTower.Location);
    }


    private bool IsNearEnemyTower()
    {
        var HostileTowers = state.SiteTracker.HostileTowers();
        var sortedTowers = SiteTracker.SitesByProximityTo(HostileTowers, queen.Location);
        var nearbyTowers = sortedTowers.Where(t => t.Location.Proximity(queen.Location) <= t.AttackRange).ToList();

        Console.Error.WriteLine($"Queen is near {nearbyTowers.Count()} enemy towers.");

        foreach (Site t in nearbyTowers)
        {
            Console.Error.WriteLine($"Tower at {t.Location.X}, {t.Location.Y} is {t.Location.Proximity(queen.Location)} units away");
        }

        return nearbyTowers.Count() > 0;
    }
}


class SiteUpdate
{
    public int id { get; set; }
    public int goldRemaining { get; set; }
    public int maxMineSize { get; set; }
    public int structureType { get; set; }
    public int owner { get; set; }
    public int param1 { get; set; }
    public int param2 { get; set; }
}

class Site
{
    public int Id { get; }
    public Location Location { get; }
    public int Radius { get; }
    public int GoldRemaining { get; private set; }
    public int MaxMineSize { get; private set; }
    public int StructureType { get; private set; }
    public int Owner { get; private set; }
    public int Param1 { get; private set; }
    public int Param2 { get; private set; }

    public Site(int id, Location location, int radius)
    {
        Id = id;
        Location = location;
        Radius = radius;
    }

    public void Update(SiteUpdate update)
    {
        GoldRemaining = update.goldRemaining;
        MaxMineSize = update.maxMineSize;
        StructureType = update.structureType;
        Owner = update.owner;
        Param1 = update.param1;
        Param2 = update.param2;
    }

    public bool IsNotOurs()
    {
        return Owner != 0;
    }

    public bool IsFriendly()
    {
        return Owner == 0;
    }

    public bool IsHostile()
    {
        return Owner == 1;
    }

    public bool IsEmpty()
    {
        return StructureType == -1;
    }

    public bool IsGoldMine()
    {
        return StructureType == 0;
    }

    public bool IsTower()
    {
        return StructureType == 1;
    }

    public bool isBarracks()
    {
        return StructureType == 2;
    }
}

class Tower : Site
{
    public int HP
    {
        get
        {
            return Param1;
        }
    }

    public int AttackRange
    {
        get
        {
            return Param2;
        }
    }

    public Tower(int id, Location location, int radius) : base(id, location, radius) { }


}

class SiteTracker
{
    private Dictionary<int, Site> Sites;

    public SiteTracker()
    {
        Sites = new Dictionary<int, Site>();
    }

    public Site Get(int id)
    {
        return Sites[id];
    }

    public void Add(Site site)
    {
        Sites.Add(site.Id, site);
    }

    public void Update(SiteUpdate update)
    {
        if (update.structureType == 1)
        {
            Sites[update.id] = new Tower(update.id, Sites[update.id].Location, Sites[update.id].Radius);
            Sites[update.id].Update(update);
        }
        Sites[update.id].Update(update);
    }

    public void Update(List<SiteUpdate> updates)
    {
        foreach (var update in updates)
        {
            Update(update);
        }
    }

    public List<Site> AllSites()
    {
        return Sites.Values.ToList();
    }

    public List<Site> SitesByProximity(Location location)
    {
        return SitesByProximityTo(AllSites(), location);
    }

    public List<Site> FriendlySites()
    {
        return Sites.Values.Where(s => s.IsFriendly()).ToList();
    }

    public List<Site> HostileSites()
    {
        return Sites.Values.Where(s => s.IsHostile()).ToList();
    }

    public List<Tower> HostileTowers()
    {
        return HostileSites().OfType<Tower>().ToList();
    }

    public List<Site> SitesOwnedBy(int owner)
    {
        return Sites.Values.Where(s => s.Owner == owner).ToList();
    }

    public List<Site> SitesNotOwnedBy(int owner)
    {
        return Sites.Values.Where(s => s.Owner != owner).ToList();
    }

    public List<Site> SitesWithGold()
    {
        return Sites.Values.Where(s => s.GoldRemaining > 30).ToList();
    }

    public List<Site> SafeBuildSites()
    {
        var hostileTowers = HostileTowers();

        return Sites.Values.Where(s => {
            // if (hostileTowers.Count() == 0)
            // {
            //     return s.IsNotOurs();
            // }
            foreach (var t in hostileTowers)
            {
                if (s.Location.Proximity(t.Location) <= t.AttackRange)
                {
                    return false;
                }
            }
            return  s.IsNotOurs();
        }).ToList();
    }

    public List<Site> OwnedBarracks()
    {
        return Sites.Values.Where(s => s.isBarracks() & s.IsFriendly()).ToList();
    }

    public static List<Site> SitesByProximityTo(List<Site> sites, Location location)
    {
        return sites.OrderBy(s => s.Location.Proximity(location)).ToList();
    }

    public static List<Tower> SitesByProximityTo(List<Tower> sites, Location location)
    {
        return sites.OrderBy(s => s.Location.Proximity(location)).ToList();
    }
}

class Trainer
{
    public int Gold { get; set; }


}


class GameState
{
    public int Gold { get; private set; }
    public bool ShouldSave { get; private set; } = true;
    readonly int TargetSavings = 150;
    readonly int MinSavings = 20;

    public Queen Queen;
    public SiteTracker SiteTracker { get; private set; }
    public UnitTracker UnitTracker;

    // public UnitTracker UnitTracker;

    public GameState()
    {
        Gold = 0;
        Queen = null;
        SiteTracker = new SiteTracker();
        UnitTracker = new UnitTracker(new List<Unit>());
    }

    public void UpdateSites(List<SiteUpdate> updates)
    {
        SiteTracker.Update(updates);
    }

    public void UpdateGold(int gold)
    {
        Gold = gold;

        if (gold > TargetSavings)
        {
            Console.Error.WriteLine("We saved PLENTY");
            ShouldSave = false;
        }
        else if (gold < MinSavings)
        {
            Console.Error.WriteLine("We spent PLENTY");
            ShouldSave = true;
        }
    }

    // public void Update(Queen queen, List<Unit> units) {
    //     Queen = queen;
    //     UnitTracker.Update(units);      
    // }

    // Maybe goes outside of GameState
    public void QueenTurn()
    {
        Queen.TakeTurn(this);
    }

    // Maybe goes outside of GameState
    public void TrainTurn()
    {
        if (ShouldSave)
        {
            Console.WriteLine("TRAIN");
        }
        else
        {
            var ownedBarracks = SiteTracker.OwnedBarracks();
            if (ownedBarracks.Count > 0)
            {
                Console.WriteLine($"TRAIN {ownedBarracks[0].Id}");
            }
            else
            {
                Console.WriteLine("TRAIN");
            }
        }
    }
}

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        // var gold = 0;
        // Queen queen = null;
        // var sites = new Dictionary<int, Site>();
        var gameState = new GameState();
        var units = new List<Unit>();

        // init read
        string[] inputs;
        int numSites = int.Parse(Console.ReadLine());
        for (int i = 0; i < numSites; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int siteId = int.Parse(inputs[0]);
            int x = int.Parse(inputs[1]);
            int y = int.Parse(inputs[2]);
            int radius = int.Parse(inputs[3]);
            // sites.Add(siteId, new Site(siteId, new Location(x, y), radius));
            gameState.SiteTracker.Add(new Site(siteId, new Location(x, y), radius));
        }

        // game loop
        while (true)
        {
            units = new List<Unit>();
            inputs = Console.ReadLine().Split(' ');
            int gold = int.Parse(inputs[0]);
            int touchedSite = int.Parse(inputs[1]); // -1 if none
            // gold = inputGold;
            gameState.UpdateGold(gold);
            // gameState.Gold = gold;
            for (int i = 0; i < numSites; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int siteId = int.Parse(inputs[0]);
                int goldRemaining = int.Parse(inputs[1]); // -1 if unknown
                int maxMineSize = int.Parse(inputs[2]); // -1 if unknown
                int structureType = int.Parse(inputs[3]); // -1 = No structure, 0 = Goldmine, 1 = Tower, 2 = Barracks
                int owner = int.Parse(inputs[4]); // -1 = No structure, 0 = Friendly, 1 = Enemy
                int param1 = int.Parse(inputs[5]);
                int param2 = int.Parse(inputs[6]);

                gameState.SiteTracker.Update(new SiteUpdate
                {
                    id = siteId,
                    goldRemaining = goldRemaining,
                    maxMineSize = maxMineSize,
                    structureType = structureType,
                    owner = owner,
                    param1 = param1,
                    param2 = param2,
                });
            }
            int numUnits = int.Parse(Console.ReadLine());
            for (int i = 0; i < numUnits; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]);
                int owner = int.Parse(inputs[2]);
                int unitType = int.Parse(inputs[3]); // -1 = QUEEN, 0 = KNIGHT, 1 = ARCHER, 2 = GIANT
                int health = int.Parse(inputs[4]);
                if (owner == 0 && unitType == -1)
                {
                    if (gameState.Queen == null)
                    {
                        gameState.Queen = new Queen(i, owner, new Location(x, y), unitType, health);
                    }
                    else
                    {
                        var update = new QueenUpdate(new Location(x, y), health, touchedSite);
                        gameState.Queen.Update(update, gameState);
                    }
                }
                else
                {
                    units.Add(new Unit(i, owner, new Location(x, y), unitType, health));
                }
                gameState.UnitTracker = new UnitTracker(units);
            }


            var HostileTowers = gameState.SiteTracker.HostileTowers();
            foreach (var tower in HostileTowers)
            {
                Console.Error.WriteLine($"TOWER: p1-{tower.Param1} p2-{tower.Param2}");
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            gameState.QueenTurn();
            gameState.TrainTurn();


            // First line: A valid queen action
            // Second line: A set of training instructions
            // Console.WriteLine("WAIT");
            // Console.WriteLine("TRAIN");
        }
    }
}
