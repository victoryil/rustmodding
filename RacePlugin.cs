using Oxide.Core.Libraries.Covalence;
using Oxide.Core;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("RacePlugin", "victoryil", "1.0.0")]
    [Description("Gestor de eventos para carreras de coches")]
    public class RacePlugin : CovalencePlugin
    {
        private RaceManager raceManager;
        private RaceStats raceStats;
        private RaceUI raceUI;

        private const string EventsDataFile = "RacePlugin/Events";
        private const string StatsDataFile = "RacePlugin/Stats";
        private const string ConfigFileName = "RacePlugin";
        private RacePluginConfig config;

        private Dictionary<string, RaceEvent> savedEvents = new Dictionary<string, RaceEvent>();
        private Dictionary<string, int> playerStats = new Dictionary<string, int>(); // Estadísticas por jugador


        private void Init()
        {
            LoadConfig();
            LoadData();

            raceManager = new RaceManager(savedEvents, this);
            raceStats = new RaceStats(playerStats);
            raceUI = new RaceUI();

            AddCovalenceCommand("race", nameof(RaceCommand));
        }

        protected override void LoadDefaultConfig()
        {
            Config.WriteObject(RacePluginConfig.GetDefaultConfig(), true);
        }

        private void LoadConfig()
        {
            config = Config.ReadObject<RacePluginConfig>();
            if (config == null)
            {
                config = RacePluginConfig.GetDefaultConfig();
                SaveConfig();
            }
        }

        private void LoadData()
        {
            // Crear la carpeta de datos si no existe
            if (!Interface.Oxide.DataFileSystem.ExistsDatafile(EventsDataFile))
            {
                savedEvents = new Dictionary<string, RaceEvent>();
                Interface.Oxide.DataFileSystem.WriteObject(EventsDataFile, savedEvents);
            }
            else
            {
                savedEvents = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<string, RaceEvent>>(EventsDataFile);
            }

            if (!Interface.Oxide.DataFileSystem.ExistsDatafile(StatsDataFile))
            {
                playerStats = new Dictionary<string, int>();
                Interface.Oxide.DataFileSystem.WriteObject(StatsDataFile, playerStats);
            }
            else
            {
                playerStats = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<string, int>>(StatsDataFile);
            }
        }


        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject(EventsDataFile, savedEvents);
            Interface.Oxide.DataFileSystem.WriteObject(StatsDataFile, playerStats);
        }

        private void Unload()
        {
            SaveData(); // Guardar datos al descargar el plugin
        }

        private void RaceCommand(IPlayer player, string command, string[] args)
        {
            if (args.Length == 0)
            {
                raceUI.ShowHelp(player);
                return;
            }

            switch (args[0].ToLower())
            {
                case "create":
                    raceManager.CreateRace(player, raceUI);
                    break;
                case "save":
                    raceManager.SaveRace(player, raceUI);
                    break;
                case "set":
                    if (args.Length < 3)
                    {
                        raceUI.Notify(player, "Uso: /race set {finish|checkpoint} {flags}");
                        return;
                    }
                    HandleSetCommand(player, args);
                    break;

                case "edit":
                    if (args.Length < 3)
                    {
                        raceUI.Notify(player, "Uso: /race edit {finish|checkpoint} {flags}");
                        return;
                    }
                    HandleEditCommand(player, args);
                    break;
                case "cancel":
                    raceManager.CancelCreation(player, raceUI);
                    break;
                case "start":
                    if (args.Length < 2)
                    {
                        raceUI.Notify(player,
                            "Debes especificar el nombre de una carrera guardada para iniciarla. Ejemplo: /race start carrera1");
                        return;
                    }

                    raceManager.StartRace(player, args[1], GetConnectedPlayers(), raceUI);
                    break;

                case "join":
                    raceManager.JoinRace(player, raceUI);
                    break;
                case "stats":
                    raceStats.ShowStats(player);
                    break;
                case "positions":
                    raceManager.ShowPositions(player, raceUI);
                    break;
                case "list":
                    raceManager.ListRaces(player, raceUI);
                    break;
                default:
                    raceUI.ShowHelp(player);
                    break;
            }
        }

        private List<IPlayer> GetConnectedPlayers()
        {
            return new List<IPlayer>(players.Connected);
        }

        private Vector3 ToVector3(GenericPosition position)
        {
            return new Vector3(position.X, position.Y, position.Z);
        }

        private void HandleSetCommand(IPlayer player, string[] args)
        {
            var subCommand = args[1].ToLower();

            switch (subCommand)
            {
                case "finish":
                    if (args.Length < 4 || args[2].ToLower() != "radius" || !float.TryParse(args[3], out float finishRadius))
                    {
                        raceUI.Notify(player, "Uso: /race set finish radius {valor}");
                        return;
                    }

                    var finishPosition = ToVector3(player.Position());
                    raceManager.SetFinishPoint(player, finishPosition, finishRadius, raceUI);
                    break;

                case "checkpoint":
                    if (args.Length < 4 || args[2].ToLower() != "radius" || !float.TryParse(args[3], out float checkpointRadius))
                    {
                        raceUI.Notify(player, "Uso: /race add checkpoint radius {valor}");
                        return;
                    }

                    var checkpointPosition = ToVector3(player.Position());
                    raceManager.AddCheckpoint(player, checkpointPosition, checkpointRadius, raceUI);
                    break;

                default:
                    raceUI.Notify(player, "Subcomando desconocido. Usa 'finish' o 'checkpoint'.");
                    break;
            }
        }
        private void HandleEditCommand(IPlayer player, string[] args)
        {
            var subCommand = args[1].ToLower();

            switch (subCommand)
            {
                case "finish":
                    if (args.Length < 4 || args[2].ToLower() != "radius" || !float.TryParse(args[3], out float newFinishRadius))
                    {
                        raceUI.Notify(player, "Uso: /race edit finish radius {valor}");
                        return;
                    }

                    raceManager.EditFinishRadius(player, newFinishRadius, raceUI);
                    break;

                case "checkpoint":
                    if (args.Length < 5 || !int.TryParse(args[2], out int index) || args[3].ToLower() != "radius" || !float.TryParse(args[4], out float newCheckpointRadius))
                    {
                        raceUI.Notify(player, "Uso: /race edit checkpoint {índice} radius {valor}");
                        return;
                    }

                    raceManager.EditCheckpointRadius(player, index, newCheckpointRadius, raceUI);
                    break;

                default:
                    raceUI.Notify(player, "Subcomando desconocido. Usa 'finish' o 'checkpoint'.");
                    break;
            }
        }


        
        // Clase interna RaceManager
        private class RacePluginConfig
        {
            public int MinWaitTime { get; set; } = 300; // Tiempo máximo de espera para jugadores (en segundos)

            public int AutoStartTime { get; set; } =
                60; // Tiempo para iniciar la carrera tras alcanzar el mínimo de jugadores (en segundos)

            public static RacePluginConfig GetDefaultConfig()
            {
                return new RacePluginConfig();
            }
        }

        // Clase interna RaceManager
        public class RaceManager
        {
            private bool raceActive = false;
            private List<RacePlayer> participants = new List<RacePlayer>();

            private int laps = 3;

            // Almacén de carreras guardadas
            private Dictionary<string, RaceEvent> savedEvents = new Dictionary<string, RaceEvent>();
            private RaceEvent editingEvent = null; // Evento en edición
            private RaceEvent activeEvent = null;  // Evento activo (en curso)


            private readonly RacePlugin plugin;

            public RaceManager(Dictionary<string, RaceEvent> loadedEvents, RacePlugin plugin)
            {
                savedEvents = loadedEvents;
                this.plugin = plugin;
            }

            public void ListRaces(IPlayer player, RaceUI raceUI)
            {
                if (savedEvents.Count == 0)
                {
                    raceUI.Notify(player, "No hay carreras guardadas actualmente.");
                    return;
                }

                string raceList = "Carreras guardadas:\n";
                foreach (var raceName in savedEvents.Keys)
                {
                    raceList += $"- {raceName}\n";
                }

                raceUI.Notify(player, raceList);
            }

            public void CreateRace(IPlayer player, RaceUI raceUI)
            {
                if (editingEvent != null)
                {
                    raceUI.Notify(player, "Ya estás creando una carrera. Usa '/race save' para guardarla o '/race cancel' para cancelarla.");
                    return;
                }

                editingEvent = new RaceEvent();
                raceUI.Notify(player, "Modo de creación de carrera iniciado.");
                ShowCreationMenu(player, raceUI);
            }


            public void ShowCreationMenu(IPlayer player, RaceUI raceUI)
            {
                string menu = "Configuración de la carrera:\n" +
                              $"1. Nombre: {editingEvent.Name ?? "Sin asignar"}\n" +
                              $"2. Jugadores mínimos: {editingEvent.MinPlayers}\n" +
                              $"3. Jugadores máximos: {editingEvent.MaxPlayers}\n" +
                              $"4. Segundos: {(editingEvent.TimeLimit > 0 ? editingEvent.TimeLimit.ToString() : "Sin asignar")}\n" +
                              "Escribe:\n" +
                              "- /race set name {nombre}\n" +
                              "- /race set min {número}\n" +
                              "- /race set max {número}\n" +
                              "- /race set seconds {número}\n" +
                              "- /race save para guardar los requisitos previos.";
                raceUI.Notify(player, menu);
            }

            public void SetRaceOption(IPlayer player, string option, string value, RaceUI raceUI)
            {
                if (editingEvent == null)
                {
                    raceUI.Notify(player, "No estás creando ninguna carrera. Usa '/race create' para empezar.");
                    return;
                }

                switch (option.ToLower())
                {
                    case "name":
                        editingEvent.Name = value;
                        raceUI.Notify(player, $"Nombre de la carrera configurado a '{value}'.");
                        break;
                    case "min":
                        if (int.TryParse(value, out int minPlayers))
                        {
                            editingEvent.MinPlayers = minPlayers;
                            raceUI.Notify(player, $"Número mínimo de jugadores configurado a {minPlayers}.");
                        }
                        else
                        {
                            raceUI.Notify(player, "Por favor, introduce un número válido para el mínimo de jugadores.");
                        }

                        break;
                    case "max":
                        if (int.TryParse(value, out int maxPlayers))
                        {
                            editingEvent.MaxPlayers = maxPlayers;
                            raceUI.Notify(player, $"Número máximo de jugadores configurado a {maxPlayers}.");
                        }
                        else
                        {
                            raceUI.Notify(player, "Por favor, introduce un número válido para el máximo de jugadores.");
                        }

                        break;
                    case "seconds":
                        if (int.TryParse(value, out int seconds))
                        {
                            editingEvent.TimeLimit = seconds;
                            raceUI.Notify(player, $"Tiempo límite configurado a {seconds} segundos.");
                        }
                        else
                        {
                            raceUI.Notify(player, "Por favor, introduce un número válido para los segundos.");
                        }

                        break;
                    default:
                        raceUI.Notify(player, "Opción desconocida. Usa 'name', 'min', 'max' o 'seconds'.");
                        break;
                }

                ShowCreationMenu(player, raceUI); // Actualiza el menú
            }

            public void SaveRace(IPlayer player, RaceUI raceUI)
            {
                if (editingEvent == null)
                {
                    raceUI.Notify(player, "No hay ninguna carrera en creación para guardar.");
                    return;
                }

                if (string.IsNullOrEmpty(editingEvent.Name))
                {
                    raceUI.Notify(player, "Debes asignar un nombre a la carrera antes de guardarla. Usa '/race set name {nombre}'.");
                    return;
                }

                if (savedEvents.ContainsKey(editingEvent.Name))
                {
                    raceUI.Notify(player, $"Ya existe un evento con el nombre '{editingEvent.Name}'. Usa otro nombre o edítalo.");
                    return;
                }

                savedEvents[editingEvent.Name] = editingEvent;
                Interface.Oxide.DataFileSystem.WriteObject(EventsDataFile, savedEvents);
                raceUI.Notify(player, $"Carrera '{editingEvent.Name}' guardada correctamente.");
                editingEvent = null;
            }


            public void EditRace(IPlayer player, string name, RaceUI raceUI)
            {
                if (!savedEvents.ContainsKey(name))
                {
                    raceUI.Notify(player, $"No se encontró una carrera con el nombre '{name}'.");
                    return;
                }

                if (editingEvent != null)
                {
                    raceUI.Notify(player, "Ya estás editando o creando una carrera. Usa '/race save' o '/race cancel' antes de continuar.");
                    return;
                }

                editingEvent = savedEvents[name];
                raceUI.Notify(player, $"Editando carrera '{name}'. Usa '/race save' para guardar los cambios o '/race cancel' para descartarlos.");
            }


            public void CancelCreation(IPlayer player, RaceUI raceUI)
            {
                if (editingEvent == null)
                {
                    raceUI.Notify(player, "No hay ninguna carrera en creación o edición para cancelar.");
                    return;
                }

                editingEvent = null;
                raceUI.Notify(player, "La creación o edición de la carrera ha sido cancelada.");
            }


            public void StartRace(IPlayer player, string raceName, List<IPlayer> connectedPlayers, RaceUI raceUI)
            {
                if (activeEvent != null)
                {
                    raceUI.Notify(player, "Ya hay una carrera activa.");
                    return;
                }

                if (!savedEvents.ContainsKey(raceName))
                {
                    raceUI.Notify(player, $"No se encontró una carrera con el nombre '{raceName}'. Usa '/race list' para ver las disponibles.");
                    return;
                }

                activeEvent = savedEvents[raceName];
                participants.Clear();

                // Log básico en el chat
                raceUI.NotifyAll(connectedPlayers, $"¡Carrera '{raceName}' iniciada con los siguientes parámetros!");
                raceUI.NotifyAll(connectedPlayers, $"- Vueltas: {activeEvent.Laps}");
                raceUI.NotifyAll(connectedPlayers, $"- Jugadores mínimos: {activeEvent.MinPlayers}");
                raceUI.NotifyAll(connectedPlayers, $"- Jugadores máximos: {activeEvent.MaxPlayers}");
                raceUI.NotifyAll(connectedPlayers, $"- Tiempo límite: {activeEvent.TimeLimit} segundos");

                raceUI.Notify(player, "Los jugadores pueden unirse usando '/race join'.");
            }



            public void JoinRace(IPlayer player, RaceUI raceUI)
            {
                if (!raceActive)
                {
                    raceUI.Notify(player, "No hay una carrera activa. Usa '/race start {nombre}' para iniciar una.");
                    return;
                }

                if (activeEvent == null)
                {
                    raceUI.Notify(player,
                        "No hay un evento de carrera válido configurado. Asegúrate de iniciar correctamente una carrera.");
                    return;
                }

                if (participants.Exists(p => p.Player.Id == player.Id))
                {
                    raceUI.Notify(player, "Ya estás en la carrera.");
                    return;
                }

                if (participants.Count >= activeEvent.MaxPlayers)
                {
                    raceUI.Notify(player, $"La carrera ya está llena. Máximo de jugadores: {activeEvent.MaxPlayers}.");
                    return;
                }

                participants.Add(new RacePlayer(player));
                raceUI.Notify(player,
                    $"Te has unido a la carrera. Jugadores actuales: {participants.Count}/{activeEvent.MaxPlayers}");

                // Comprobar si se cumple el mínimo de jugadores
                CheckStartConditions(raceUI);
            }


            private void CheckStartConditions(RaceUI raceUI)
            {
                if (participants.Count >= activeEvent.MinPlayers)
                {
                    raceUI.NotifyAll(plugin.GetConnectedPlayers(),
                        $"¡Se alcanzó el mínimo de jugadores ({activeEvent.MinPlayers})! La carrera comenzará en {plugin.config.AutoStartTime} segundos.");

                    // Temporizador para iniciar la carrera automáticamente
                    plugin.timer.Once(plugin.config.AutoStartTime, () => { StartCountdown(raceUI); });
                }
                else if (participants.Count == 1) // Primer jugador que se une
                {
                    raceUI.NotifyAll(plugin.GetConnectedPlayers(),
                        $"Esperando a que se unan al menos {activeEvent.MinPlayers} jugadores. Tiempo máximo: {plugin.config.MinWaitTime} segundos.");

                    // Temporizador de espera máxima
                    plugin.timer.Once(plugin.config.MinWaitTime, () =>
                    {
                        if (participants.Count >= editingEvent.MinPlayers) return;
                        raceUI.NotifyAll(plugin.GetConnectedPlayers(),
                            "No se alcanzó el número mínimo de jugadores. La carrera será cancelada.");
                        CancelRace(raceUI);
                    });
                }
            }

            private void StartCountdown(RaceUI raceUI)
            {
                raceUI.NotifyAll(plugin.GetConnectedPlayers(), "¡La carrera está comenzando ahora!");
                raceActive = true;

                // Aquí se puede implementar la lógica para iniciar la carrera (teletransporte, etc.)
            }

            public void ShowPositions(IPlayer player, RaceUI raceUI)
            {
                if (!raceActive)
                {
                    raceUI.Notify(player, "No hay una carrera activa.");
                    return;
                }

                string positions = "Posiciones actuales:\n";
                for (int i = 0; i < participants.Count; i++)
                {
                    positions += $"{i + 1}. {participants[i].Player.Name} - Vueltas: {participants[i].LapsCompleted}\n";
                }

                raceUI.Notify(player, positions);
            }

            public void CancelRace(RaceUI raceUI)
            {
                if (!raceActive)
                {
                    raceUI.NotifyAll(plugin.GetConnectedPlayers(), "No hay una carrera activa para cancelar.");
                    return;
                }

                // Resetear el estado de la carrera
                raceActive = false;
                participants.Clear();
                activeEvent = null;

                // Notificar a los jugadores
                raceUI.NotifyAll(plugin.GetConnectedPlayers(), "La carrera ha sido cancelada.");
            }
            public void EndRace(RaceUI raceUI)
            {
                if (activeEvent == null)
                {
                    raceUI.NotifyAll(plugin.GetConnectedPlayers(), "No hay una carrera activa para finalizar.");
                    return;
                }

                raceUI.NotifyAll(plugin.GetConnectedPlayers(), $"¡La carrera '{activeEvent.Name}' ha terminado!");
                activeEvent = null;
                participants.Clear();
            }

            
            private void EnsureEditing(IPlayer player, RaceUI raceUI)
            {
                if (editingEvent == null)
                {
                    raceUI.Notify(player, "No estás editando ninguna carrera. Usa '/race edit {nombre}' para comenzar a editar.");
                    throw new InvalidOperationException("No hay carrera en edición.");
                }
            }
            

            public void SetFinishPoint(IPlayer player, Vector3 position, float radius, RaceUI raceUI)
            {
                EnsureEditing(player, raceUI);

                editingEvent.FinishPoint = new Checkpoint(position, radius);
                raceUI.Notify(player, $"Meta configurada en {position} con un radio de {radius} metros.");
            }
            public void AddCheckpoint(IPlayer player, Vector3 position, float radius, RaceUI raceUI)
            {
                EnsureEditing(player, raceUI);
                var checkpoint = new Checkpoint(position, radius);
                editingEvent.Checkpoints.Add(checkpoint);
                int checkpointIndex = editingEvent.Checkpoints.Count - 1;

                // Notificar al jugador
                raceUI.Notify(player, $"Checkpoint {checkpointIndex} añadido en {position} con un radio de {radius} metros.");
            }

            public void EditFinishRadius(IPlayer player, float newRadius, RaceUI raceUI)
            {
                EnsureEditing(player, raceUI);

                if (editingEvent.FinishPoint == null)
                {
                    raceUI.Notify(player, "La meta no está configurada. Usa '/race set finish radius {valor}' primero.");
                    return;
                }

                editingEvent.FinishPoint.Radius = newRadius;
                raceUI.Notify(player, $"Radio de la meta actualizado a {newRadius} metros.");
            }
            public void EditCheckpointRadius(IPlayer player, int index, float newRadius, RaceUI raceUI)
            {
                EnsureEditing(player, raceUI);

                if (index < 0 || index >= editingEvent.Checkpoints.Count)
                {
                    raceUI.Notify(player, $"El índice {index} está fuera de rango. Hay {editingEvent.Checkpoints.Count} checkpoints configurados.");
                    return;
                }

                editingEvent.Checkpoints[index].Radius = newRadius;
                raceUI.Notify(player, $"Radio del checkpoint {index} actualizado a {newRadius} metros.");
            }

            
            

            public bool IsEditing()
            {
                return editingEvent != null;
            }

            public bool IsActive()
            {
                return activeEvent != null;
            }

        }

        // Clase interna RacePlayer
        public class RacePlayer
        {
            public IPlayer Player { get; }
            public int LapsCompleted { get; set; } = 0;

            public RacePlayer(IPlayer player)
            {
                Player = player;
            }
        }

        // Clase interna RaceStats
        public class RaceStats
        {
            private Dictionary<string, int> playerWins = new Dictionary<string, int>();

            public RaceStats(Dictionary<string, int> loadedStats)
            {
                playerWins = loadedStats;
            }

            public void RecordWin(IPlayer player)
            {
                if (!playerWins.ContainsKey(player.Id))
                {
                    playerWins[player.Id] = 0;
                }

                playerWins[player.Id]++;
                Interface.Oxide.DataFileSystem.WriteObject(StatsDataFile, playerWins); // Guardar en disco
            }

            public void ShowStats(IPlayer player)
            {
                if (!playerWins.ContainsKey(player.Id))
                {
                    player.Reply("No tienes estadísticas registradas.");
                    return;
                }

                player.Reply($"Victorias totales: {playerWins[player.Id]}");
            }
        }

        // Clase interna RaceUI
        public class RaceUI
        {
            public void ShowHelp(IPlayer player)
            {
                player.Reply("Comandos disponibles:");
                player.Reply("/race start {name} - Iniciar una carrera creada con un nombre específico.");
                player.Reply("/race join - Unirse a una carrera activa.");
                player.Reply("/race stats - Ver estadísticas personales.");
                player.Reply("/race positions - Ver posiciones actuales.");
                player.Reply("/race create - Iniciar la creación de una carrera.");
                player.Reply("/race set {name|min|max|seconds} {valor} - Configurar opciones de la carrera.");
                player.Reply("/race save - Guardar la carrera en creación.");
                player.Reply("/race edit {name} - Editar una carrera guardada.");
                player.Reply("/race cancel - Cancelar la creación o edición de una carrera.");
                player.Reply("/race list - Listar todas las carreras guardadas.");
            }

            public void Notify(IPlayer player, string message)
            {
                player.Reply(message);
            }

            public void NotifyAll(List<IPlayer> connectedPlayers, string message)
            {
                foreach (var player in connectedPlayers)
                {
                    player.Reply(message);
                }
            }
        }

        public class Checkpoint
        {
            public Vector3 Position { get; set; }
            public float Radius { get; set; }

            public Checkpoint(Vector3 position, float radius)
            {
                Position = position;
                Radius = radius;
            }
        }

        public class RaceEvent
        {
            public string Name { get; set; }
            public int MinPlayers { get; set; } = 1;
            public int MaxPlayers { get; set; } = 10;
            public int TimeLimit { get; set; } = 0;
            public int Laps { get; set; } = 3;
            public List<Vector3> StartPositions { get; set; } = new List<Vector3>();
            public Checkpoint FinishPoint { get; set; }
            public List<Checkpoint> Checkpoints { get; set; } = new List<Checkpoint>();
        }
    }
}