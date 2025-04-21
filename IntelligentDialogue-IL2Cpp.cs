using MelonLoader;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Il2CppTMPro;
using Il2CppScheduleOne.Persistence;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using HarmonyLib;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Dialogue;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.Police;
using Il2CppScheduleOne.Employees;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.VoiceOver;
using Il2CppScheduleOne.Law;
using Newtonsoft.Json.Linq;
using Il2CppInterop.Runtime.Injection;

[assembly: MelonInfo(typeof(IntelligentDialogue.IntelligentDialogue), IntelligentDialogue.BuildInfo.Name, IntelligentDialogue.BuildInfo.Version, IntelligentDialogue.BuildInfo.Author, IntelligentDialogue.BuildInfo.DownloadLink)]
[assembly: MelonColor()]
[assembly: MelonOptionalDependencies("FishNet.Runtime")]
[assembly: MelonGame("TVGS", "Schedule I")]
namespace IntelligentDialogue
{
    public static class BuildInfo
    {
        public const string Name = "IntelligentDialogue";
        public const string Description = "Smart!";
        public const string Author = "XOWithSauce";
        public const string Company = null;
        public const string Version = "1.1";
        public const string DownloadLink = null;
    }

    public delegate void RoleActionDelegate(NPC npc);

    public class Role
    {
        public string Name { get; set; }
        public Dictionary<int, RoleActionDelegate> Actions = new Dictionary<int, RoleActionDelegate>();
        public List<string> Descriptions = new List<string>();
        public List<string> Moods = new List<string>();

    }

    public static class RoleRegistry
    {
        public static Dictionary<string, Role> Roles = new Dictionary<string, Role>();

        public static void Initialize()
        {
            Roles["Employee"] = new Role
            {
                Name = "Employee",
                Actions = new Dictionary<int, RoleActionDelegate>
                    {
                        { 1, EmployeeAction1 },
                        { 2, EmployeeAction2 },
                        { 3, EmployeeAction3 },
                        { 4, EmployeeAction4 },
                        { 5, EmployeeAction5 },

                    },
                Descriptions = new List<string>
                    {
                        "Action1 (Make Suprised sound), ",
                        "Action2 (Make Disagreeing sound), ",
                        "Action3 (Start being annoyed), ",
                        "Action4 (Try resigning from the job), ",
                        "Action5 (Increase your Daily Wage), ",
                    },
                Moods = new List<string>
                    {
                        "Stressed about deadlines",
                        "Pretending to care",
                        "Trying to stay positive",
                        "Counting minutes to lunch break",
                        "Annoyed by management"
                    }
            };

            Roles["Officer"] = new Role
            {
                Name = "Officer",
                Actions = new Dictionary<int, RoleActionDelegate>
                    {
                        { 1, OfficerAction1 },
                        { 2, OfficerAction2 },
                        { 3, OfficerAction3 },
                        { 4, OfficerAction4 },
                        { 5, OfficerAction5 },

                    },
                Descriptions = new List<string>
                    {
                        "Action1 (Talk to Police Radio), ",
                        "Action2 (Make sounds that command the player), ",
                        "Action3 (Increase Suspicion level), ",
                        "Action4 (Player committed crime, try to arrest), ",
                        "Action5 (Body search the nearest Player), ",
                    },
                Moods = new List<string>
                    {
                        "Suspicious of everyone",
                        "Calm but alert",
                        "Tired of crime",
                        "In command mode",
                        "Ready to escalate"
                    }
            };

            Roles["Dealer"] = new Role
            {
                Name = "Dealer",
                Actions = new Dictionary<int, RoleActionDelegate>
                    {
                        { 1, DealerAction1 },
                        { 2, DealerAction2 },
                        { 3, DealerAction3 },
                        { 4, DealerAction4 },
                        { 5, DealerAction5 },

                    },
                Descriptions = new List<string>
                    {
                        "Action1 (Increase aggression), ",
                        "Action2 (Make Annoyed sounds), ",
                        "Action3 (Make Thinking sounds), ",
                        "Action4 (Decrease Dealer Payment Cut), ",
                        "Action5 (Increase Dealer Payment Cut), ",
                    },
                Moods = new List<string>
                    {
                        "Paranoid",
                        "Smooth-talking",
                        "Cocky and overconfident",
                        "On edge",
                        "Chill but watching you"
                    }
            };

            Roles["Citizen"] = new Role
            {
                Name = "Citizen",
                Actions = new Dictionary<int, RoleActionDelegate>
                    {
                        { 1, CitizenAction1 },
                        { 2, CitizenAction2 },
                        { 3, CitizenAction3 },
                        { 4, CitizenAction4 },
                        { 5, CitizenAction5 },

                    },
                Descriptions = new List<string>
                    {
                        "Action1 (Start Panicking), ",
                        "Action2 (Thank the Player), ",
                        "Action3 (Make Acknowledge sounds), ",
                        "Action4 (Fall down Funnily), ",
                        "Action5 (Walk away), ",
                    },
                Moods = new List<string>
                    {
                        "Worried about safety",
                        "Happy and satisfied",
                        "Innocent and curious",
                        "Fed up with the system",
                        "Friendly but cautious"
                    }
            };
        }

        private static void EmployeeAction1(NPC npc) => npc.PlayVO(EVOLineType.Surprised);
        private static void EmployeeAction2(NPC npc) => npc.PlayVO(EVOLineType.No);
        private static void EmployeeAction3(NPC npc)
        {
            npc.Avatar.EmotionManager.AddEmotionOverride("Annoyed", "base_emotion", 0f, 0);
            npc.PlayVO(EVOLineType.Annoyed);
        }
        private static void EmployeeAction4(NPC npc)
        {
            if (UnityEngine.Random.Range(0, 100) >= 70)
            {
                Employee employee = npc as Employee;
                var fireMethod = typeof(Employee).GetMethod("Fire", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                fireMethod?.Invoke(employee, null);
            }
        }
        private static void EmployeeAction5(NPC npc)
        {
            Employee employee = npc as Employee;
            if (employee.DailyWage < 2000f)
                employee.DailyWage += 50f;
        }

        private static void OfficerAction1(NPC npc) => npc.PlayVO(EVOLineType.PoliceChatter);
        private static void OfficerAction2(NPC npc) => npc.PlayVO(EVOLineType.Command);
        private static void OfficerAction3(NPC npc)
        {
            PoliceOfficer officer = npc as PoliceOfficer;
            if (officer.Suspicion < 0.95f)
                officer.Suspicion += 0.05f;
        }
        private static void OfficerAction4(NPC npc)
        {
            PoliceOfficer officer = npc as PoliceOfficer;
            if (officer == null) return;

            Player p = Player.GetClosestPlayer(officer.transform.position, out float dist);

            if (p != null)
            {
                try
                {
                    p.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Arresting);
                    officer.BeginFootPursuit_Networked(p.NetworkObject, false);
                }
                catch (Exception ex)
                {
                    //MelonLogger.Error($"[OfficerAction4] Error while starting pursuit: {ex.Message}");
                }
            }
        }
        private static void OfficerAction5(NPC npc)
        {
            PoliceOfficer officer = npc as PoliceOfficer;
            if (officer == null) return;

            Player p = Player.GetClosestPlayer(officer.transform.position, out float dist);
            if (p != null)
            {
                try
                {
                    p.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.None);
                    officer.BeginBodySearch_Networked(p.NetworkObject);
                }
                catch (Exception ex)
                {
                    //MelonLogger.Error($"[OfficerAction4] Error while starting pursuit: {ex.Message}");
                }
            }

        }

        private static void DealerAction1(NPC npc) => npc.OverrideAggression(1f);
        private static void DealerAction2(NPC npc) => npc.PlayVO(EVOLineType.Annoyed);
        private static void DealerAction3(NPC npc) => npc.PlayVO(EVOLineType.Think);
        private static void DealerAction4(NPC npc)
        {
            Dealer dealer = npc as Dealer;
            if (dealer.Cut > 0.05f)
                dealer.Cut -= 0.05f;
        }
        private static void DealerAction5(NPC npc)
        {
            Dealer dealer = npc as Dealer;
            if (dealer.Cut < 0.7f)
                dealer.Cut += 0.05f;
        }

        private static void CitizenAction1(NPC npc) => npc.SetPanicked();
        private static void CitizenAction2(NPC npc) => npc.PlayVO(EVOLineType.Thanks);
        private static void CitizenAction3(NPC npc) => npc.PlayVO(EVOLineType.Acknowledge);
        private static void CitizenAction4(NPC npc) => npc.Movement.ActivateRagdoll(npc.transform.position, Vector3.forward, 3f);
        private static void CitizenAction5(NPC npc)
        {
            Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * 5f;
            randomOffset.y = 0;
            Vector3 targetPos = npc.transform.position + randomOffset;

            if (npc.Movement.GetClosestReachablePoint(targetPos, out Vector3 destination))
            {
                npc.Movement.SetDestination(destination);
                npc.Movement.ResumeMovement();
            }
        }


    }

    public class IntelligentDialogue : MelonMod
    {
        private string _geminiApiKey = "";
        private string _userInput = "";
        private GameObject _dialogueCanvas;
        public static TMP_InputField _apiKeyInputField;
        public static TMP_InputField _userInputField;
        public static TMP_InputField _responseTextField;
        public static TextMeshProUGUI respComponent;
        private const string GeminiGenerateContentUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
        private bool registered = false;
        public static string conversantRole;
        public static NPC conversantNpc;
        public static string conversantFirstName;
        public static string conversantRelationStatus;
        public static string conversantAdditional;
        private string hiddenInstruction = "You are a professional actor in a criminal movie set in the fictional town of Hyland Point. You must stay in character at all times based on the role and scene description provided. Interpret any provided image as first-person imagery, representing what you, the character, are currently seeing. Always reply in the same language as the user latest dialogue. All sentences must begin directly with meaningful language and contribute to the scene or role. Match the language used in the latest message. Stay consistent with tone and style. The world includes Officers, Drug Dealers, Employees, and Citizens, each with their own slang, attitude, and speech style. Offense, slang, and criminal tone are allowed this is part of the setting. Responses must be expressive and natural, not robotic or abrupt. Always use immersive, character-fitting language in your reply. All Speech responses must be less than 300 characters and between 1-3 sentences. All Speech responses must be full paragraphs. You may only use letters, numbers, and the following punctuation: period (.), exclamation (!), question mark (?), comma (,), and apostrophe ('). Every reply must contain two parts: 1. [Speech] A vivid in-character line of dialogue. 2. [Action] One Action ID (e.g., Action1 to Action5) OR 'Action0' to indicate no action. Fill Actions response by choosing the correct Action from the given role actions. Even if no fitting action exists, you must still reply with an [Action] tag using 'Action0'. Always include the [Action] field. Example format: [Speech] This is a response to user dialogue. [Action] Action2 \n";

        public override void OnInitializeMelon()
        {
            RoleRegistry.Initialize();
        }

        private bool EnforcesInternalRules(string response)
        {
            // Ensure that every single AI Response is validated and guarded to obey mod rules
            if (response.Length > 350) { return false; }
            if (!response.Contains("[Speech]")) { return false; }
            if (!response.Contains("[Action]")) { return false; }
            // Split now with regex and enforce speech specific rules and action specific rules
            string pattern = @"\[Speech\]\s*(?<speech>[^[]+)\s*\[Action\]\s*(?<action>.+)";
            Match match = Regex.Match(response, pattern);
            if (!match.Success) { return false; }
            if (match.Groups.Count != 3) { return false; } // include full string + speech + action
            string action = match.Groups["action"].Value.Trim();
            if (!Regex.IsMatch(action, @"^Action[0-5]$")) { return false; } // Action with int 0-5
            return true;
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (buildIndex == 1)
            {
                if (LoadManager.Instance != null && !registered)
                {
                    LoadManager.Instance.onLoadComplete.AddListener((UnityEngine.Events.UnityAction)OnLoadCompleteCb);
                }
            }
            else
            {
                if (LoadManager.Instance != null && registered)
                {
                    LoadManager.Instance.onLoadComplete.RemoveListener((UnityEngine.Events.UnityAction)OnLoadCompleteCb);
                }
                registered = false;
            }
        }
        private void OnLoadCompleteCb()
        {
            if (registered) return;

            _dialogueCanvas = GameObject.Find("DialogueCanvas");
            if (_dialogueCanvas == null)
            {
                //MelonLogger.Error("DialogueCanvas not found. UI cannot be created.");
                return;
            }
            CreateGeminiUI();
            registered = true;
        }


        // Patch: InitializeDialogue(DialogueContainer)
        [HarmonyPatch(typeof(Il2CppScheduleOne.Dialogue.DialogueHandler), "InitializeDialogue", new Type[] { typeof(Il2CppScheduleOne.Dialogue.DialogueContainer) })]
        public static class Patch_InitDialogue_Container
        {
            public static bool Prefix(Il2CppScheduleOne.Dialogue.DialogueHandler __instance, Il2CppScheduleOne.Dialogue.DialogueContainer container)
            {
                return HandleDialogueInit(__instance);
            }
        }

        // Patch: InitializeDialogue(DialogueContainer, bool, string)
        [HarmonyPatch(typeof(Il2CppScheduleOne.Dialogue.DialogueHandler), "InitializeDialogue", new Type[] { typeof(Il2CppScheduleOne.Dialogue.DialogueContainer), typeof(bool), typeof(string) })]
        public static class Patch_InitDialogue_ContainerBoolString
        {
            public static bool Prefix(Il2CppScheduleOne.Dialogue.DialogueHandler __instance, Il2CppScheduleOne.Dialogue.DialogueContainer dialogueContainer, bool enableDialogueBehaviour = true, string entryNodeLabel = "ENTRY")
            {
                return HandleDialogueInit(__instance);
            }
        }

        // Patch: InitializeDialogue(string, bool, string)
        [HarmonyPatch(typeof(Il2CppScheduleOne.Dialogue.DialogueHandler), "InitializeDialogue", new Type[] { typeof(string), typeof(bool), typeof(string) })]
        public static class Patch_InitDialogue_StringBoolString
        {
            public static bool Prefix(Il2CppScheduleOne.Dialogue.DialogueHandler __instance, string dialogueContainerName, bool enableDialogueBehaviour = true, string entryNodeLabel = "ENTRY")
            {
                return HandleDialogueInit(__instance);
            }
        }

        private static bool HandleDialogueInit(DialogueHandler __instance)
        {
            _userInputField.interactable = true;
            _apiKeyInputField.interactable = true;
            NPC npc = __instance.NPC;

            if (npc == null)
            {
                //MelonLogger.Msg("Failed to get NPC Instance");
                return true;
            }

            IntelligentDialogue.conversantNpc = npc;
            IntelligentDialogue.conversantFirstName = npc.FirstName;

            float rel = npc.RelationData.RelationDelta;
            IntelligentDialogue.conversantRelationStatus =
                rel < 1f ? "Bad" :
                rel < 2f ? "Poor" :
                rel < 3f ? "Neutral" :
                rel < 4f ? "Good" :
                rel >= 4f ? "Great" :
                "Unknown"; // Default


            if (npc is Employee)
                IntelligentDialogue.conversantRole = "Employee";

            else if (npc is PoliceOfficer)
            {
                IntelligentDialogue.conversantRole = "Officer";
                Player p = Player.GetClosestPlayer(npc.transform.position, out float dist);
                if (p.CrimeData.Crimes.Count > 0)
                {
                    IntelligentDialogue.conversantAdditional = "Your dialogue conversant is a known criminal and has following crimes on record: ";
                    foreach (Il2CppSystem.Collections.Generic.KeyValuePair<Crime, int> c in p.CrimeData.Crimes)
                    {
                        IntelligentDialogue.conversantAdditional += $" {c.key.CrimeName}";
                    }
                    IntelligentDialogue.conversantAdditional += ". ";

                }
            }

            else if (npc is Dealer d)
            {
                IntelligentDialogue.conversantRole = "Dealer";

                if (d.IsRecruited)
                    IntelligentDialogue.conversantAdditional = "You are recruited as dealer by the dialogue conversant Boss. ";
                else
                    IntelligentDialogue.conversantAdditional = "Your dialogue conversant is a competing drug dealer. ";
            }

            else
            {
                IntelligentDialogue.conversantRole = "Citizen";

                Customer c = npc.GetComponent<Customer>();
                if (c != null && npc.RelationData.Unlocked && Customer.UnlockedCustomers.Contains(c))
                {
                    string favDay = c.CustomerData.PreferredOrderDay.ToString();
                    float add = c.CurrentAddiction;
                    string adj =
                        add < 0.2f ? "not" :
                        add < 0.4f ? "somewhat" :
                        add < 0.6f ? "" :
                        add < 0.8f ? "very" :
                        add >= 0.8f ? "extremely" :
                        ""; // Default
                    IntelligentDialogue.conversantAdditional = $"Your dialogue conversant is your personal drug dealer and you are {adj} addicted to their drugs. You like ordering from them on every {c.CustomerData.PreferredOrderDay} and you spend {c.CustomerData.MinWeeklySpend} dollars on drugs weekly. ";
                }
                else
                {
                    IntelligentDialogue.conversantAdditional = "Your dialogue conversant is a dealer, but you dont consume their drugs. ";
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(Il2CppScheduleOne.Dialogue.DialogueHandler), "EndDialogue")]
        public static class Diag_EndDiag_Patch
        {
            public static bool Prefix(DialogueHandler __instance)
            {
                _userInputField.interactable = false;
                _apiKeyInputField.interactable = false;
                IntelligentDialogue.conversantRole = "";
                IntelligentDialogue.conversantAdditional = "";
                IntelligentDialogue.conversantRelationStatus = "";
                conversantNpc = null;
                IntelligentDialogue.respComponent.text = "";
                return true;
            }
        }

        public string GetPromptEmbed()
        {
            string availableActions = "";
            string randomMood = "";
            if (RoleRegistry.Roles.TryGetValue(conversantRole, out var role))
            {
                foreach (string desc in role.Descriptions)
                {
                    availableActions += desc;
                }
                randomMood = role.Moods[UnityEngine.Random.Range(0, role.Moods.Count)];
            }

            string roleRevealInstruction = $"You are {conversantFirstName} Your mood is: {randomMood}. Your relations with the conversant are {conversantRelationStatus}. {conversantAdditional} Your Role is: {conversantRole}. Role Actions: {availableActions}. ";
            return hiddenInstruction + roleRevealInstruction;
        }
        private void CreateGeminiUI()
        {
            GameObject inputGO = new GameObject("UserInputField");
            inputGO.transform.SetParent(_dialogueCanvas.transform, false);
            inputGO.transform.SetAsLastSibling();
            RectTransform rt = inputGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(500, 100);
            rt.anchoredPosition = new Vector2(0f, 350f);
            UnityEngine.UI.Image bg = inputGO.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(1, 1, 1, 0.8f);
            _userInputField = inputGO.AddComponent<TMP_InputField>();
            _userInputField.textViewport = CreateTextArea(inputGO.transform, out TextMeshProUGUI textComponent, Color.black);
            _userInputField.textComponent = textComponent;
            _userInputField.placeholder = CreatePlaceholder(inputGO.transform, "Type your message");
            _userInputField.characterLimit = 200;
            _userInputField.lineType = TMP_InputField.LineType.MultiLineNewline;
            _userInputField.contentType = TMP_InputField.ContentType.Standard;
            _userInputField.onValueChanged.AddListener((UnityEngine.Events.UnityAction<string>)OnPlayerInputChange);
            _userInputField.interactable = false;

            GameObject outputGo = new GameObject("ResponseField");
            outputGo.transform.SetParent(_dialogueCanvas.transform, false);
            outputGo.transform.SetAsLastSibling();
            RectTransform rtr = outputGo.AddComponent<RectTransform>();
            rtr.anchorMin = new Vector2(0.5f, 0.5f);
            rtr.anchorMax = new Vector2(0.5f, 0.5f);
            rtr.pivot = new Vector2(0.5f, 0.5f);
            rtr.sizeDelta = new Vector2(500, 350);
            rtr.anchoredPosition = new Vector2(0f, 120f);
            Image bgr = outputGo.AddComponent<Image>();
            bgr.color = Color.clear;
            _responseTextField = outputGo.AddComponent<TMP_InputField>();
            _responseTextField.image.color = Color.clear;
            _responseTextField.textViewport = CreateTextArea(outputGo.transform, out TextMeshProUGUI textComponentOut, Color.white);
            respComponent = textComponentOut;
            respComponent.fontSize = 18f;
            _responseTextField.textComponent = respComponent;
            _responseTextField.characterLimit = 300;
            _responseTextField.lineType = TMP_InputField.LineType.MultiLineNewline;
            _responseTextField.contentType = TMP_InputField.ContentType.Standard;
            _responseTextField.readOnly = true;
            _responseTextField.interactable = false;

            GameObject keyField = new GameObject("InputKeyField");
            keyField.transform.SetParent(_dialogueCanvas.transform, false);
            keyField.transform.SetAsLastSibling();
            RectTransform rti = keyField.AddComponent<RectTransform>();
            rti.anchorMin = new Vector2(0.5f, 0.5f);
            rti.anchorMax = new Vector2(0.5f, 0.5f);
            rti.pivot = new Vector2(0.5f, 0.5f);
            rti.sizeDelta = new Vector2(300, 40);
            rti.anchoredPosition = new Vector2(-800f, 300f);
            Image bgi = keyField.AddComponent<Image>();
            bgi.color = new Color(1f, 0.45f, 0.3f, 0.4f);
            _apiKeyInputField = keyField.AddComponent<TMP_InputField>();
            _apiKeyInputField.textViewport = CreateTextArea(keyField.transform, out TextMeshProUGUI textComponenti, Color.black);
            textComponenti.fontSize = 12f;
            _apiKeyInputField.textComponent = textComponenti;
            _apiKeyInputField.characterLimit = 50;
            _apiKeyInputField.placeholder = CreatePlaceholder(keyField.transform, "Insert API Key");
            _apiKeyInputField.lineType = TMP_InputField.LineType.SingleLine;
            _apiKeyInputField.contentType = TMP_InputField.ContentType.Password;
            _apiKeyInputField.onValueChanged.AddListener((UnityEngine.Events.UnityAction<string>)OnApiKeyInputChange);
            _apiKeyInputField.interactable = false;
        }
        private void OnPlayerInputChange(string input)
        {
            _userInput = input;
        }
        private void OnApiKeyInputChange(string input)
        {
            _geminiApiKey = input;
        }

        private RectTransform CreateTextArea(Transform parent, out TextMeshProUGUI textComp, Color color)
        {
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(parent, false);
            RectTransform rt = textGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(10, 6);
            rt.offsetMax = new Vector2(-10, -6);

            textComp = textGO.AddComponent<TextMeshProUGUI>();
            textComp.fontSize = 18f;
            textComp.color = color;
            textComp.alignment = TextAlignmentOptions.MidlineLeft;

            return rt;
        }
        private Graphic CreatePlaceholder(Transform parent, string content)
        {
            GameObject placeholderGO = new GameObject("Placeholder");
            placeholderGO.transform.SetParent(parent, false);
            RectTransform rt = placeholderGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(10, 6);
            rt.offsetMax = new Vector2(-10, -6);

            TextMeshProUGUI placeholderText = placeholderGO.AddComponent<TextMeshProUGUI>();
            placeholderText.text = content;
            placeholderText.fontSize = 12f;
            placeholderText.fontStyle = FontStyles.Italic;
            placeholderText.color = Color.black;
            placeholderText.alignment = TextAlignmentOptions.MidlineLeft;

            return placeholderText;
        }
        public override void OnUpdate()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(_userInput) && !string.IsNullOrEmpty(_geminiApiKey))
            {
                //MelonLogger.Msg("input" + _userInput + "\n\n");
                MelonCoroutines.Start(SendRequestToGeminiWithImage(_userInput, (response) =>
                {
                    //MelonLogger.Msg(response);
                    _userInput = "";
                    _userInputField.text = "";
                    if (response == null || response == string.Empty) { return; }
                    if (!EnforcesInternalRules(response))
                    {
                        MelonLogger.Warning("Gemini Response does not enforce mod rules and was discarded.");
                        return;
                    }
                    MelonCoroutines.Start(ResponseCb(response));
                }));
            }
        }

        private IEnumerator ResponseCb(string response)
        {
            //MelonLogger.Msg(response);
            string pattern = @"\[Speech\]\s*(?<speech>[^[]+?)\s*\[Action\]\s*(?<action>[^\[\]]+)";
            Match match = Regex.Match(response, pattern);
            string speech = match.Groups["speech"].Value.Trim();
            string action = match.Groups["action"].Value.Trim();
            if (_responseTextField != null)
            {
                _responseTextField.text = speech;
            }

            if (int.TryParse(action.Substring(6), out int actionId))
            {
                if (string.IsNullOrEmpty(conversantRole))
                {
                    //MelonLogger.Msg("Invalid Conversant role or Unassigned");
                    yield break;
                }

                if (RoleRegistry.Roles.TryGetValue(conversantRole, out var role) &&
                    role.Actions.TryGetValue(actionId, out var actionFn))
                {
                    actionFn.Invoke(conversantNpc);
                }
                else
                {
                    //MelonLogger.Msg($"No action {actionId} defined for role {conversantRole}");
                }
            }
            yield return null;

        }
        private IEnumerator SendRequestToGeminiWithImage(string prompt, System.Action<string> onResponse)
        {
            string urlWithKey = $"{GeminiGenerateContentUrl}?key={_geminiApiKey}";
            Transform npcTransform = conversantNpc.transform;
            string systemInstruction = GetPromptEmbed();
            if (npcTransform == null)
            {
                //MelonLogger.Error("NPC Transform not found!");
                yield break;
            }

            SnapshotCamera snapshotCamera = SnapshotCamera.MakeSnapshotCamera(npcTransform, "NPCSnapshotCamera");
            Texture2D snapshot = snapshotCamera.TakeSnapshot(Color.clear, 1920, 1080);

            UnityEngine.Object.Destroy(snapshotCamera.gameObject);

            byte[] imageBytes;
            string mimeType;

            //imageBytes = snapshot.EncodeToPNG();
            //mimeType = "image/png";
            imageBytes = UnityEngine.ImageConversion.EncodeToJPG(snapshot, 8);
            mimeType = "image/jpg";

            string base64Image = System.Convert.ToBase64String(imageBytes);

            string jsonPayload = $@"{{
              ""system_instruction"": {{
                ""parts"": [
                  {{
                    ""text"": ""{systemInstruction}""
                  }}
                ]
              }},
              ""contents"": [
                {{
                  ""parts"": [
                    {{
                      ""inline_data"": {{
                        ""mime_type"": ""{mimeType}"",
                        ""data"": ""{base64Image}""
                      }}
                    }},
                    {{
                      ""text"": ""{prompt}""
                    }}
                  ]
                }}
              ],
              ""generationConfig"": {{
                ""temperature"": 1.5,
                ""maxOutputTokens"": 350,
                ""topP"": 0.25,
                ""topK"": 30
              }}
            }}";


            UnityWebRequest request = null;
            try
            {
                request = new UnityWebRequest(urlWithKey, UnityWebRequest.kHttpVerbPOST);
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    MelonLogger.Error($"Gemini API request failed: {request.error}");
                    onResponse?.Invoke($"Gemini API error: {request.error}");
                }
                else
                {
                    string responseText = request.downloadHandler.text;
                    try
                    {
                        JObject jsonResponse = JObject.Parse(responseText);
                        string generatedText = jsonResponse["candidates"][0]["content"]["parts"][0]["text"].ToString().Trim();
                        //MelonLogger.Msg("Generated response\n " + generatedText);
                        onResponse?.Invoke(generatedText);
                    }
                    catch (System.Exception e)
                    {
                        MelonLogger.Error($"Error parsing Gemini response: {e.Message}\nResponse: {responseText}");
                        onResponse?.Invoke("Failed to parse Gemini API response.");
                    }
                }
            }
            finally
            {
                request?.Dispose();
            }

            UnityEngine.Object.Destroy(snapshot);
        }

        #region Image Utility 
        [RegisterTypeInIl2Cpp]
        public class SnapshotCamera : MonoBehaviour
        {
            public SnapshotCamera(IntPtr ptr) : base(ptr) { }

            public SnapshotCamera() : base(ClassInjector.DerivedConstructorPointer<SnapshotCamera>())
                => ClassInjector.DerivedConstructorBody(this);

            private Camera cam;

            public static SnapshotCamera MakeSnapshotCamera(Transform tr, string name = "Snapshot Camera")
            {
                GameObject snapshotCameraGO = new(name);
                Camera cam = snapshotCameraGO.AddComponent<Camera>();

                // Configure the Camera
                cam.orthographic = false;
                cam.orthographicSize = 1;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.clear;
                cam.nearClipPlane = 0.1f;
                cam.enabled = false;

                // Configure position
                snapshotCameraGO.transform.parent = tr;
                snapshotCameraGO.transform.localPosition = Vector3.zero + Vector3.forward * 1.02f;
                snapshotCameraGO.transform.localPosition += Vector3.up * 1.78f;
                Vector3 player = Player.GetClosestPlayer(snapshotCameraGO.transform.position, out float dist).transform.position;
                snapshotCameraGO.transform.LookAt(player + Vector3.up * 1.06f);
                // Add a SnapshotCamera component to the GameObject
                SnapshotCamera snapshotCamera = snapshotCameraGO.AddComponent<SnapshotCamera>();
                snapshotCamera.cam = cam;

                // Return the SnapshotCamera
                return snapshotCamera;
            }
            public Texture2D TakeSnapshot(Color backgroundColor, int width, int height)
            {
                cam.backgroundColor = backgroundColor;
                cam.targetTexture = RenderTexture.GetTemporary(width, height, 24);
                cam.Render();

                RenderTexture previouslyActiveRenderTexture = RenderTexture.active;
                RenderTexture.active = cam.targetTexture;
                Texture2D texture = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.ARGB32, false);
                texture.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
                texture.Apply(false);

                RenderTexture.active = previouslyActiveRenderTexture;

                cam.targetTexture = null;
                RenderTexture.ReleaseTemporary(cam.targetTexture);

                return texture;
            }
        }
        #endregion
    }
}