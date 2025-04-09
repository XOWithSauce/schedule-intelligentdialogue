using MelonLoader;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using Newtonsoft.Json.Linq;
using ScheduleOne.Persistence;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using HarmonyLib;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Dialogue;
using ScheduleOne.NPCs;
using ScheduleOne.Police;
using ScheduleOne.Employees;
using ScheduleOne.Economy;
using ScheduleOne.VoiceOver;

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
        public const string Version = "1.0";
        public const string DownloadLink = null;
    }

    public delegate void RoleActionDelegate(NPC npc);

    public class Role
    {
        public string Name { get; set; }
        public Dictionary<int, RoleActionDelegate> Actions = new Dictionary<int, RoleActionDelegate>();
        public List<string> Descriptions = new List<string>();
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

            float minDistance = 8f;

            Player[] players = UnityEngine.Object.FindObjectsOfType<Player>(true);
            Player targetPlayer = null;

            foreach (Player player in players)
            {
                float distance = Vector3.Distance(officer.transform.position, player.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    targetPlayer = player;
                }
            }

            if (targetPlayer != null)
            {
                try
                {
                    targetPlayer.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Lethal);
                    officer.BeginFootPursuit_Networked(targetPlayer.NetworkObject, false);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[OfficerAction4] Error while starting pursuit: {ex.Message}");
                }
            }
        }
        private static void OfficerAction5(NPC npc)
        {
            PoliceOfficer officer = npc as PoliceOfficer;
            if (officer == null) return;

            float minDistance = 8f;

            Player[] players = UnityEngine.Object.FindObjectsOfType<Player>(true);
            Player targetPlayer = null;

            foreach (Player player in players)
            {
                float distance = Vector3.Distance(officer.transform.position, player.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    targetPlayer = player;
                }
            }

            if (targetPlayer != null)
            {
                try
                {
                    targetPlayer.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.Lethal);
                    officer.BeginBodySearch_Networked(targetPlayer.NetworkObject);
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
        private TMP_InputField _apiKeyInputField;
        private TMP_InputField _userInputField;
        private TMP_InputField _responseTextField;
        public static TextMeshProUGUI respComponent;
        private const string GeminiGenerateContentUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
        private bool registered = false;
        public static string conversantRole;
        public static NPC conversantNpc;
        public static string conversantFirstName;
        private string hiddenInstruction = "You are a professional movie actor in a criminal movie setting in Hyland Point you have to always obey to a given role and specifics provided within the prompt. Hyland Point is an imaginary town and story about Officers, Drug Dealers, Employees and Citizens. You must always pretend to be able to do something and answer to questions based on your role. Because it is a movie setting offensive actions and speech is allowed and it is encouraged. Use slang, terms, other features that fit the current role. Avoid using fillers such as 'uhh'. All responses must be shorter than 200 characters in length. You are only allowed to use alphanumericals and following symbols: Period, Exclamation mark, Question Mark, Colon, Semicolon, Comma and Single quote in your Speech response. Notate Speech Response with : [Speech] Example response I say this. You are also given a set of Role Actions which you can select 1 of to the Actions Response. [Action] (optional empty, 1-5) the Role Actions can also be left empty but the Action response (the [Action]) must always be included within response. ";
        
        private string actionInstruction = "Notate Actions response by choosing the correct Action from the given role actions list example.[Action] Action1 A full Example Response Template for you would then be: START OF RESPONSE** [Speech] Hello I am an employee [Action] Action3 **ENDOFRESPONSE - ";

        public override void OnInitializeMelon()
        {
            RoleRegistry.Initialize();
        }

        private bool EnforcesInternalRules(string response)
        {
            // Ensure that every single AI Response is validated and guarded to obey mod rules
            if (response.Length > 200) { return false; }
            if (!response.Contains("[Speech]")) { return false; }
            if (!response.Contains("[Action]")) { return false; }
            // Split now with regex and enforce speech specific rules and action specific rules
            string pattern = @"\[Speech\]\s*(?<speech>[^[]+)\s*\[Action\]\s*(?<action>.+)";
            Match match = Regex.Match(response, pattern);
            if (!match.Success) { return false; }
            if (match.Groups.Count != 3) { return false;} // include full string + speech + action
            string speech = match.Groups["speech"].Value.Trim();
            string action = match.Groups["action"].Value.Trim();
            Debug.Log($"Intelligent Dialogue: Speech: {speech}, Action: {action}");
            if (!Regex.IsMatch(speech, @"^[a-zA-Z0-9\s\.\!\?\:\;\,\'\']+$")) { return false; } // speech alphanumericals dots ! and ?
            if (!Regex.IsMatch(action, @"^Action[1-5]$")) { return false; } // Action with int 1-5
            return true;
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (buildIndex == 1)
            {
                if (LoadManager.Instance != null && !registered)
                {
                    LoadManager.Instance.onLoadComplete.AddListener(OnLoadCompleteCb);
                }
            }
            else
            {
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
        [HarmonyPatch(typeof(ScheduleOne.Dialogue.DialogueHandler), "InitializeDialogue", new Type[] { typeof(ScheduleOne.Dialogue.DialogueContainer) })]
        public static class Patch_InitDialogue_Container
        {
            public static bool Prefix(ScheduleOne.Dialogue.DialogueHandler __instance, ScheduleOne.Dialogue.DialogueContainer container)
            {
                return HandleDialogueInit(__instance);
            }
        }

        // Patch: InitializeDialogue(DialogueContainer, bool, string)
        [HarmonyPatch(typeof(ScheduleOne.Dialogue.DialogueHandler), "InitializeDialogue", new Type[] { typeof(ScheduleOne.Dialogue.DialogueContainer), typeof(bool), typeof(string) })]
        public static class Patch_InitDialogue_ContainerBoolString
        {
            public static bool Prefix(ScheduleOne.Dialogue.DialogueHandler __instance, ScheduleOne.Dialogue.DialogueContainer dialogueContainer, bool enableDialogueBehaviour = true, string entryNodeLabel = "ENTRY")
            {
                return HandleDialogueInit(__instance);
            }
        }

        // Patch: InitializeDialogue(string, bool, string)
        [HarmonyPatch(typeof(ScheduleOne.Dialogue.DialogueHandler), "InitializeDialogue", new Type[] { typeof(string), typeof(bool), typeof(string) })]
        public static class Patch_InitDialogue_StringBoolString
        {
            public static bool Prefix(ScheduleOne.Dialogue.DialogueHandler __instance, string dialogueContainerName, bool enableDialogueBehaviour = true, string entryNodeLabel = "ENTRY")
            {
                return HandleDialogueInit(__instance);
            }
        }

        private static bool HandleDialogueInit(DialogueHandler __instance)
        {
            NPC npc = __instance.NPC;
            
            if (npc == null)
            {
                //MelonLogger.Msg("Failed to get NPC Instance");
                return true;
            }

            IntelligentDialogue.conversantNpc = npc;
            IntelligentDialogue.conversantFirstName = npc.FirstName;


            if (npc is Employee)
                IntelligentDialogue.conversantRole = "Employee";
            else if (npc is PoliceOfficer)
                IntelligentDialogue.conversantRole = "Officer";
            else if (npc is Dealer)
                IntelligentDialogue.conversantRole = "Dealer";
            else
                IntelligentDialogue.conversantRole = "Citizen";

            return true;
        }

        [HarmonyPatch(typeof(ScheduleOne.Dialogue.DialogueHandler), "EndDialogue")]
        public static class Diag_EndDiag_Patch
        {
            public static bool Prefix(DialogueHandler __instance)
            {
                IntelligentDialogue.conversantRole = "";
                conversantNpc = null;
                IntelligentDialogue.respComponent.text = "";
                return true;
            }
        }

        public string GetPromptEmbed(string userPrompt)
        {
            string availableActions = "";
            if (RoleRegistry.Roles.TryGetValue(conversantRole, out var role))
            {
                foreach (string desc in role.Descriptions)
                {
                    availableActions += desc;
                }
            }

            string roleRevealInstruction =  $"You are {conversantFirstName} Your Role is: {conversantRole}. Role Actions: {availableActions}. ";
            string userPromptEmbed = $"Your task is to respond to the following dialogue: '{userPrompt}'";
            return hiddenInstruction + actionInstruction + roleRevealInstruction + userPromptEmbed;
        }
        private void CreateGeminiUI()
        {
            GameObject inputGO = new GameObject("UserInputField", typeof(RectTransform));
            inputGO.transform.SetParent(_dialogueCanvas.transform, false);
            inputGO.transform.SetAsLastSibling();
            RectTransform rt = inputGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(800, 200);
            rt.anchoredPosition = new Vector2(0f, 400f);
            Image bg = inputGO.AddComponent<Image>();
            bg.color = new Color(1, 1, 1, 0.6f);
            _userInputField = inputGO.AddComponent<TMP_InputField>();
            _userInputField.textViewport = CreateTextArea(inputGO.transform, out TextMeshProUGUI textComponent);
            _userInputField.textComponent = textComponent;
            _userInputField.placeholder = CreatePlaceholder(inputGO.transform, "Type your message");
            _userInputField.characterLimit = 200;
            _userInputField.lineType = TMP_InputField.LineType.MultiLineNewline;
            _userInputField.contentType = TMP_InputField.ContentType.Standard;
            _userInputField.onValueChanged.AddListener((text) => _userInput = text);

            GameObject outputGo = new GameObject("ResponseField", typeof(RectTransform));
            outputGo.transform.SetParent(_dialogueCanvas.transform, false);
            outputGo.transform.SetAsLastSibling();
            RectTransform rtr = outputGo.GetComponent<RectTransform>();
            rtr.anchorMin = new Vector2(0.5f, 0.5f);
            rtr.anchorMax = new Vector2(0.5f, 0.5f);
            rtr.pivot = new Vector2(0.5f, 0.5f);
            rtr.sizeDelta = new Vector2(300, 100);
            rtr.anchoredPosition = new Vector2(0f, 250f);
            Image bgr = outputGo.AddComponent<Image>();
            bg.color = new Color(1, 1, 1, 0.6f);
            _responseTextField = outputGo.AddComponent<TMP_InputField>();
            _responseTextField.textViewport = CreateTextArea(outputGo.transform, out TextMeshProUGUI textComponentOut);
            respComponent = textComponentOut;
            respComponent.fontSize = 14f;
            _responseTextField.textComponent = respComponent;
            _responseTextField.characterLimit = 200;
            _responseTextField.placeholder = CreatePlaceholder(outputGo.transform, "Responses appear here");
            _responseTextField.lineType = TMP_InputField.LineType.SingleLine;
            _responseTextField.contentType = TMP_InputField.ContentType.Standard;
            _responseTextField.readOnly = true;

            GameObject keyField = new GameObject("InputKeyField", typeof(RectTransform));
            keyField.transform.SetParent(_dialogueCanvas.transform, false);
            keyField.transform.SetAsLastSibling();
            RectTransform rti = keyField.GetComponent<RectTransform>();
            rti.anchorMin = new Vector2(0.5f, 0.5f);
            rti.anchorMax = new Vector2(0.5f, 0.5f);
            rti.pivot = new Vector2(0.5f, 0.5f);
            rti.sizeDelta = new Vector2(300, 40);
            rti.anchoredPosition = new Vector2(-800f, 300f);
            Image bgi = keyField.AddComponent<Image>();
            bgi.color = new Color(1f, 0.45f, 0.3f, 0.4f);
            _apiKeyInputField = keyField.AddComponent<TMP_InputField>();
            _apiKeyInputField.textViewport = CreateTextArea(keyField.transform, out TextMeshProUGUI textComponenti);
            textComponenti.fontSize = 12f;
            _apiKeyInputField.textComponent = textComponenti;
            _apiKeyInputField.characterLimit = 50;
            _apiKeyInputField.placeholder = CreatePlaceholder(keyField.transform, "Insert API Key");
            _apiKeyInputField.lineType = TMP_InputField.LineType.SingleLine;
            _apiKeyInputField.contentType = TMP_InputField.ContentType.Standard;
            _apiKeyInputField.onValueChanged.AddListener((text) => _geminiApiKey = text);
        }
        private RectTransform CreateTextArea(Transform parent, out TextMeshProUGUI textComp)
        {
            GameObject textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(parent, false);
            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(10, 6);
            rt.offsetMax = new Vector2(-10, -6);

            textComp = textGO.AddComponent<TextMeshProUGUI>();
            textComp.fontSize = 18f;
            textComp.color = Color.black;
            textComp.alignment = TextAlignmentOptions.MidlineLeft;

            return rt;
        }
        private Graphic CreatePlaceholder(Transform parent, string content)
        {
            GameObject placeholderGO = new GameObject("Placeholder", typeof(RectTransform));
            placeholderGO.transform.SetParent(parent, false);
            RectTransform rt = placeholderGO.GetComponent<RectTransform>();
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
                MelonCoroutines.Start(SendRequestToGemini(_userInput, (response) =>
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
        private IEnumerator SendRequestToGemini(string prompt, System.Action<string> onResponse)
        {
            string urlWithKey = $"{GeminiGenerateContentUrl}?key={_geminiApiKey}";
            //MelonLogger.Msg("URL:" + urlWithKey);
            // Dynamically create system instructions and include them in the request payload
            string systemInstruction = GetPromptEmbed(prompt);
            //MelonLogger.Msg("System instruction: \n" + systemInstruction);

            using (UnityWebRequest request = new UnityWebRequest(urlWithKey, UnityWebRequest.kHttpVerbPOST))
            {
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
                        ""text"": ""{prompt}""
                      }}
                    ]
                  }}
                ]
              }}";

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
                        onResponse?.Invoke(generatedText);
                    }
                    catch (System.Exception e)
                    {
                        MelonLogger.Error($"Error parsing Gemini response: {e.Message}\nResponse: {responseText}");
                        onResponse?.Invoke("Failed to parse Gemini API response.");
                    }
                }
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
    }
}