using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace DungeonGame.Quests
{
    // A paged journal stays readable without growing one unbounded text/list panel.
    public sealed class QuestJournal : MonoBehaviour
    {
        public QuestController owner;
        public GameObject panel;
        public TMP_Text heading;
        public TMP_Text body;
        private int page;
        private QuestService subscribed;
        private PlayerControlState controls;
        public bool IsOpen => panel != null && panel.activeInHierarchy;
        private void Start()
        {
            controls = owner.GetComponent<PlayerControlState>();
            owner.Initialize();
            subscribed = owner.Service;
            subscribed.Changed += Refresh;
        }
        public void Toggle() { if (IsOpen) Close(); else Open(); }
        public void Open()
        {
            if (controls == null || !controls.GameplayAllowed) return;
            panel.SetActive(true);
            transform.SetAsLastSibling();
            Refresh();
            controls.Synchronize();
            owner.GetComponent<PlayerCombat>()?.CancelAttack();
        }
        public void Close() { if (panel != null) panel.SetActive(false); controls?.Synchronize(); }
        public void Next() { page++; Refresh(); }
        public void Previous() { page--; Refresh(); }
        public void Refresh()
        {
            if (!IsOpen || owner.Service == null) return;
            var quests = owner.Service.Definitions.ToArray();
            if (quests.Length == 0) { heading.text = "Quest journal"; body.text = "No adventures recorded."; return; }
            page = (page % quests.Length + quests.Length) % quests.Length;
            var quest = quests[page];
            var service = owner.Service;
            var status = service.Status(quest);
            heading.text = quest.title + "   (" + (page + 1) + "/" + quests.Length + ")";
            var text = new StringBuilder();
            string color = status == QuestStatus.Completed ? "7EC985" : status == QuestStatus.Active ? "F0D27C" : "CE9385";
            text.AppendLine("<color=#" + color + ">" + status + "</color>\n").AppendLine(quest.summary).AppendLine();
            if (status == QuestStatus.NotStarted)
            {
                text.AppendLine(service.CanStart(quest, out var reason) ? "Speak to the quest giver to begin." : reason);
            }
            var stage = service.CurrentStage(quest);
            if (stage != null)
            {
                text.AppendLine(stage.journal).AppendLine();
                foreach (var objective in stage.objectives)
                    text.AppendLine(service.Count(quest, objective) + "/" + objective.count + "  " + objective.description);
                if (service.Ready(quest)) text.AppendLine("\nReturn to your contact to continue.");
            }
            if (status == QuestStatus.Completed) text.AppendLine("Adventure complete. Rewards received.");
            text.AppendLine("\nRewards: " + quest.experienceReward + " XP");
            foreach (var reward in quest.itemRewards) text.AppendLine(reward.count + " x " + reward.item.name);
            body.text = text.ToString();
        }
        private void Update()
        {
            if (IsOpen && (owner == null || owner.GetComponent<PlayerStats>()?.IsDead == true)) Close();
        }
        private void OnDisable() => Close();
        private void OnDestroy() { if (subscribed != null) subscribed.Changed -= Refresh; }
    }
}
