using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;

namespace WindowsFormsApp1
{
    public partial class Adventure : Form
    {
        private Player _player;
        private Monster _currentMonster;
        public Adventure()
        {
            InitializeComponent();

            _player = new Player(10, 10, 30, 0, 1);
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            _player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
            _player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_HEALING_POTION), 1));

            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void MoveTo(Location newLocation)
        {
            if (!_player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." +
                        Environment.NewLine;
                ScrollToBottomOfMessages();
                return;
            }

            _player.CurrentLocation = newLocation;

            // make buttons visible if there is a connection
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            // display current location name and description
            rtbLocation.Text = newLocation.Name + " - " + newLocation.Description + Environment.NewLine;

            // 100% heal player
            _player.CurrentHitPoints = _player.MaximumHitPoints;

            // update hitpoints in ui
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();

            // check for quests
            CheckForQuests(newLocation);

            // check for monster
            CheckForMonster(newLocation);

            // refresh inventory
            UpdateInventoryListInUI();

            //refresh quest list
            UpdateQuestListInUI();

            //refresh weapons   
            UpdateWeaponListInUI();

            //refresh potions
            UpdatePotionListInUI();
        }
        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] { inventoryItem.Details.Name, inventoryItem.Quantity.ToString() });
                }
            }
        }

        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest playerQuest in _player.Quests)
            {
                dgvQuests.Rows.Add(new[] { playerQuest.Details.Name, playerQuest.IsCompleted.ToString() });
            }
        }

        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is Weapon)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }

            if (weapons.Count == 0)
            {
                //player doesnt have any weapons
                //hide buttons
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else
            {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                cboWeapons.SelectedIndex = 0;
            }
        }

        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is HealingPotion)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion)inventoryItem.Details);
                    }
                }
            }

            if (healingPotions.Count == 0)
            {
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }
        }
        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            PlayerAttack();
            ScrollToBottomOfMessages();
            RefreshStats();
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            DrinkHealingPotion();
            ScrollToBottomOfMessages();
            //monster turn to attack
            MonsterAttack();
            ScrollToBottomOfMessages();
            //refresh stats
            RefreshStats();
        }

        private void RefreshStats()
        {
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();

            UpdateInventoryListInUI();
            UpdateWeaponListInUI();
            UpdatePotionListInUI();
        }

        private void MonsterAttack()
        {
            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

            rtbMessages.Text += "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " damage points." + Environment.NewLine;
            _player.CurrentHitPoints -= damageToPlayer;
            ScrollToBottomOfMessages();

            if (_player.CurrentHitPoints <= 0)
            {
                rtbMessages.Text += "The " + _currentMonster.Name + " killed you." + Environment.NewLine;
                rtbMessages.Text += "You die!" + Environment.NewLine;
                ScrollToBottomOfMessages();
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            RefreshStats();
        }

        private void DrinkHealingPotion()
        {
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            _player.CurrentHitPoints += potion.AmountToHeal;

            if (_player.CurrentHitPoints > _player.MaximumHitPoints)
            {
                _player.CurrentHitPoints = _player.MaximumHitPoints;
            }

            foreach (InventoryItem ii in _player.Inventory)
            {
                if (ii.Details.ID == potion.ID)
                {
                    ii.Quantity--;
                    break;
                }
            }

            rtbMessages.Text += "You drink a " + potion.Name + Environment.NewLine;
            ScrollToBottomOfMessages();
            RefreshStats();
        }

        private void PlayerAttack()
        {
            //get the currently selected weapon from the cboWeapons combo box
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            //calculate damage
            int damageToMonster = RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            //apply damage
            _currentMonster.CurrentHitPoints -= damageToMonster;

            //display text
            rtbMessages.Text += "You hit the " + _currentMonster.Name + " for " + damageToMonster.ToString() + " damage points." + Environment.NewLine;
            ScrollToBottomOfMessages();
            //check if monster is dead
            if (_currentMonster.CurrentHitPoints <= 0)
            {
                rtbMessages.Text += Environment.NewLine;
                rtbMessages.Text += "You defeated the " + _currentMonster.Name + Environment.NewLine;
                ScrollToBottomOfMessages();
                //give reward
                DefeatedMonsterGiveReward();
                ScrollToBottomOfMessages();
                //refresh stats
                RefreshStats();

                rtbMessages.Text += Environment.NewLine;
                MoveTo(_player.CurrentLocation);
            }
            else
            {
                //monster still alive
                MonsterAttack();
            }
            RefreshStats();
        }

        private void GiveLootToPlayer()
        {
            List<InventoryItem> lootedItems = new List<InventoryItem>();
            //add items to the loot
            //get items from monster loot table
            foreach (LootItem lootItem in _currentMonster.LootTable)
            {
                //comparing drop percentage
                if (RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
                {
                    //adding item to loot
                    lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                }
            }
            //if no items were selected, add default item
            if (lootedItems.Count == 0)
            {
                foreach (LootItem lootItem in _currentMonster.LootTable)
                {
                    if (lootItem.IsDefaultItem)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }
            }

            //add loot to player inventory
            foreach (InventoryItem inventoryItem in lootedItems)
            {
                _player.AddItemToInventory(inventoryItem.Details);

                if (inventoryItem.Quantity == 1)
                {
                    rtbMessages.Text += "You loot " + inventoryItem.Quantity.ToString() + " " + inventoryItem.Details.Name + Environment.NewLine;
                    ScrollToBottomOfMessages();
                }
                else
                {
                    rtbMessages.Text += "You loot " + inventoryItem.Quantity.ToString() + " " + inventoryItem.Details.NamePlural + Environment.NewLine;
                    ScrollToBottomOfMessages();
                }
            }
        }

        private void DefeatedMonsterGiveReward()
        {
            //give experience
            _player.ExperiencePoints += _currentMonster.RewardExperiencePoints;
            rtbMessages.Text += "You receive " + _currentMonster.RewardExperiencePoints + " experience points." + Environment.NewLine;
            ScrollToBottomOfMessages();
            //give gold
            _player.Gold += _currentMonster.RewardGold;
            rtbMessages.Text += "You receive " + _currentMonster.RewardGold + " gold." + Environment.NewLine;
            ScrollToBottomOfMessages();
            //give loot
            GiveLootToPlayer();
            ScrollToBottomOfMessages();
        }

        private void CheckForQuests(Location newLocation)
        {
            if (newLocation.QuestAvailableHere != null)
            {
                //check if player has quest and if it is completed
                bool playerAlreadyHasQuest = _player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest = _player.CompletedThisQuest(newLocation.QuestAvailableHere);

                if (playerAlreadyHasQuest)
                {
                    if (!playerAlreadyCompletedQuest)
                    {
                        //check if player has required items
                        bool playerHasAllItemsToCompleteQuest = _player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);


                        if (playerHasAllItemsToCompleteQuest)
                        {
                            // display text
                            rtbMessages.Text += Environment.NewLine;
                            rtbMessages.Text += "You complete the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine;
                            ScrollToBottomOfMessages();
                            // remove quest items
                            _player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);

                            // give quest reward
                            CompleteQuestGiveReward(newLocation);

                            // mark quest as completed
                            _player.MarkQuestCompleted(newLocation.QuestAvailableHere);

                        }
                    }
                }
                else
                {
                    // player doesn't have quest
                    // display messages
                    rtbMessages.Text += "You receive the " + newLocation.QuestAvailableHere.Name + " quest" + Environment.NewLine;
                    rtbMessages.Text += newLocation.QuestAvailableHere.Description + Environment.NewLine;
                    rtbMessages.Text += "To complete it, return with:" + Environment.NewLine;
                    ScrollToBottomOfMessages();
                    // items required
                    foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.Name + Environment.NewLine;
                        }
                        else
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.NamePlural + Environment.NewLine;
                        }

                    }

                    // add quest
                    rtbMessages.Text += Environment.NewLine;
                    ScrollToBottomOfMessages();
                    _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }
            }
        }

        private void CompleteQuestGiveReward(Location newLocation)
        {
            // experience and gold
            rtbMessages.Text += "You receive: " + Environment.NewLine;
            rtbMessages.Text += newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine;
            rtbMessages.Text += newLocation.QuestAvailableHere.RewardGold.ToString() + " gold" + Environment.NewLine;
            rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
            ScrollToBottomOfMessages();
            _player.ExperiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
            _player.Gold += newLocation.QuestAvailableHere.RewardGold;

            // items
            _player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);
        }

        private void CheckForMonster(Location newLocation)
        {
            if (newLocation.MonsterLivingHere != null)
            {
                rtbMessages.Text += "You see a " + newLocation.MonsterLivingHere.Name + Environment.NewLine;
                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);
                ScrollToBottomOfMessages();
                _currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage, standardMonster.RewardExperiencePoints,
                    standardMonster.RewardGold, standardMonster.CurrentHitPoints, standardMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standardMonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUsePotion.Visible = true;
                btnUseWeapon.Visible = true;
            }
            else
            {
                _currentMonster = null;

                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
            }
        }

        private void dgvInventory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvQuests_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void ScrollToBottomOfMessages()
        {
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

    }
}
