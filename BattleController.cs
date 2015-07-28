using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
	
	bool initiated = false; //Used to check if the Start button was pressed (in the main loop - UPDATE method)
	float timeElapsed; //Turn timer
	float randomTurnTimer = 1.50f;
	float randomActionTimer = 1.50f;
	Color sliderColor;

	// Counter for the number of turns per team. Used in unit tests
	int numAllies = 0;
	int numEnemies = 0;

	//TextField names
	string stateTextName = "TextCenter";
	string timerTextName = "TextTimer";
	string queueTextName = "TextQueue";
	string queueActionTextName = "TextAction";
	string allyTeamTextName = "AlliesLabelText";
	string enemyTeamTextName = "EnemiesLabelText";

	//Lists used for turn and action queues
	List<string> slidersTurnQueue = new List<string>();
	List<string> slidersActionQueue = new List<string> ();

	//List used  to identify enemies and allies
	List<string> slidersNameList = new List<string>();

	//Hashmaps
	Dictionary <string, int> speedList;		//Speeds related to player
	Dictionary <string, Text> textFields;	//TextFields related to their names
	Dictionary <string, Slider> sliders;	//Sliders related to their names

	//Input: speed for each ally or enemy in combat, passed as a script parameter 
	public int Ally1Speed;
	public int Ally2Speed;
	public int Ally3Speed;
	public int Ally4Speed;
	public int Enemy1Speed;
	public int Enemy2Speed;
	public int Enemy3Speed;
	public int Enemy4Speed;

	//Loading of speed parameters in the speed hashmap
	void loadSpeedList(){
		speedList = new Dictionary<string,int> ();
		speedList.Add ("Ally1", Ally1Speed);
		speedList.Add ("Ally2", Ally2Speed);
		speedList.Add ("Ally3", Ally3Speed);
		speedList.Add ("Ally4", Ally4Speed);
		speedList.Add ("Enemy1", Enemy1Speed);
		speedList.Add ("Enemy2", Enemy2Speed);
		speedList.Add ("Enemy3", Enemy3Speed);
		speedList.Add ("Enemy4", Enemy4Speed);
	}
	
	//INPUT:Variables used to simulate idle time or animations
	public float minRndRange;
	public float maxRndRange;

	//INPUT: variable used to set initial timer value
	public float timeElapsedDefault;

	//When the object -to which this script has been attached- is created, START method is triggered
	void Start(){
		//Load every Text object in the Hashmap, identifying them by their names
		textFields= new Dictionary <string, Text>();
		Text[] textFieldsArray = FindObjectsOfType (typeof(Text)) as Text[];
		foreach (Text txt in textFieldsArray) {
			textFields.Add(txt.name,txt);
		}

		//Load every slider (which represents one fighter) in the Hashmap
		sliders = new Dictionary <string,Slider>();
		Slider [] sliderArray = FindObjectsOfType (typeof(Slider)) as Slider[];
		foreach (Slider sld in sliderArray) {
			sliders.Add(sld.name,sld);
			slidersNameList.Add(sld.name);
		}
		//Save default color in sliderColor variable
		Image img = (Image)sliderArray[0].fillRect.transform.GetComponent("Image");
		sliderColor = img.color;

		//Load speeds [add them in the remaining Hashmap]
		loadSpeedList();

		//Set maximum value for the timer
		timeElapsed = timeElapsedDefault;
	}

	/* ----- BUTTON LISTENERS ---- //
	StartFight and PerformAction are the two methods triggered when pressing buttons in the interface.
	This can be set up updating the "onClick" method in any of the Button objects
	*/

	//Initiates the simulation
	public void StartFight () {
		//Set values to zero in the event that it was already running 
		ValuesToZero ();
		//Update interface to show that fight has started
		textFields[stateTextName].text = "Fight initiated";
		textFields[timerTextName].text = "TIMER: " + timeElapsed.ToString();
		initiated = true;
	}

	//Simulates an action
	public void PerformAction (int index) {
		// Right now every button does the same: stopping and reseting the timer
		if (index == 0) {
			//Check if there's someone in the turn queue
			if (slidersTurnQueue.Count>0){
				string name = slidersTurnQueue[0];
				//Check if that someone is an ally
				if (name.Contains("Ally")){
					//Reset Slider to zero and add ally to the action Queue
					ResetSliderValue(slidersTurnQueue[0]);
					slidersTurnQueue.RemoveAt(0);
					slidersActionQueue.Add(name);
					//Reset timer 
					timeElapsed = timeElapsedDefault;
				}
			}
		}
	}
	
	void Update(){
		/*Main loop that is only executed if fight started. This may have to be moved to FixedUpdate or LateUpdate 
		 * depending on whether animations, graphic particles and interactions are added in this script or not.
		*/
		if (initiated) {
			// Check if turn queue isnt empty
			if (slidersTurnQueue.Count>0){
				// If it's an ally turn, decrease turn timer
				if (slidersTurnQueue[0].Contains("Ally")){
					timeElapsed -= Time.deltaTime;
				}
				// If not, it's an enemy turn, and AI is simulated 
				else {
					ServeTurnQueue();
				}
				// If timer reaches 0, reset timer to default and ally slider to zero and remove from the queue
				if (timeElapsed<=0){
					ResetSliderValue(slidersTurnQueue[0]);
					slidersTurnQueue.RemoveAt(0);
					timeElapsed = timeElapsedDefault;
				}
			}
			//Update interface with current timer value, rounded with 2 decimals 
			timeElapsed = Mathf.Round(timeElapsed * 100.0f)/100.0f;
			textFields[timerTextName].text = "TIMER: " + timeElapsed.ToString();

			IncreaseValues(); //Method that handles the automatic increasing of sliders

			//Check if there's a pending animation in the Action Queue and simulate it
			if (slidersActionQueue.Count>0){
				ServeActionQueue();
			}

			//Finally, update interface with the newest action and turn queues
			UpdateTurnQueue();
			UpdateActionQueue();
		}

		
	}

	/* AI AND ACTION SIMULATION + QUEUE UPDATERS 
	 * 
	 * These first two methods use RNG based timers to simulate AI decisions and animations
	 * Last two methods are used to update the UI queues
	 * 
	 */

	void ServeTurnQueue(){
		//First check if turn timer has ran out
		if (randomTurnTimer <= 0) {
			//Check if there's a fighter (always an enemy) in the turn queue
			if (slidersTurnQueue.Count>0){
				string name = slidersTurnQueue[0];
				// Simulates a decision and resets the slider
				ResetSliderValue(name);
				slidersTurnQueue.RemoveAt(0);
				// Adds it to the Action Queue and resets the timer
				slidersActionQueue.Add(name);
				randomTurnTimer = Random.Range (minRndRange, maxRndRange);
			}
		}
		// If turn timer hasnt ran out, decrease its value 
		randomTurnTimer -= Time.deltaTime;
	}

	void ServeActionQueue(){
		// Same logic as the ServeTurnQueue method, but it also increases both enemy and ally counters 
		//(for unit test purposes)
		if (randomActionTimer <= 0) {
			if (slidersActionQueue.Count>0){
				string name = slidersActionQueue[0];
				slidersActionQueue.RemoveAt(0);
				if (name.Contains("Ally")){
					numAllies++;
					textFields [allyTeamTextName].text = "Allies - "+numAllies.ToString();
				}
				else {
					numEnemies++;
					textFields [enemyTeamTextName].text = "Enemies - "+numEnemies.ToString();

				}
				randomActionTimer = Random.Range (1.00f, 1.50f);
			}
		}
		randomActionTimer -= Time.deltaTime;
	}

	// ------ Both methods iterate over turn and action queues to represent them in the user interface
	void UpdateTurnQueue(){
		textFields [queueTextName].text = "TURN QUEUE";
		if (slidersTurnQueue.Count > 0) {
			for (int i = 0; i<slidersTurnQueue.Count; i++) {
				textFields [queueTextName].text += "\n" + slidersTurnQueue [i];		
			}
			textFields [stateTextName].text = slidersTurnQueue [0] + " turn";
		}
	}

	void UpdateActionQueue(){
		textFields [queueActionTextName].text = "ACTION QUEUE";
		if (slidersActionQueue.Count > 0) {
			for (int i = 0; i<slidersActionQueue.Count; i++) {
				textFields [queueActionTextName].text += "\n" + slidersActionQueue [i];		
			}
		}
	}


 //		---- Boolean methods used to check sliders state ---

	bool AllSlidersTo100(){
		bool result = true;
		for (int i = 0; i<slidersNameList.Count; i++) {
			if (sliders[slidersNameList[i]].value != 100){
				result = false;
			}
		}
		return result;
	}
	
	bool OneSliderTo100(){
		bool result = false;
		for (int i = 0; i<slidersNameList.Count; i++) {
			if (sliders[slidersNameList[i]].value == 100){
				result = true;
			}
		}
		return result;
	}

	// SLIDERS MODIFIERS -- Methods used to change values, properties and interface sections related to sliders

	void IncreaseValues(){
		// iterates over sliders to increase their value one by one
		for (int i = 0; i<slidersNameList.Count; i++) {
			IncreaseSliderValue(sliders[slidersNameList[i]],speedList[slidersNameList[i]]);
		}
	}

	//Resets all necesary values, sliders, interface textfields and queues
	void ValuesToZero(){
		for (int i = 0; i<slidersNameList.Count; i++) {
			ResetSliderValue(slidersNameList[i]);
		}
		timeElapsed = timeElapsedDefault;
		textFields [queueTextName].text = "TURN QUEUE";
		textFields [queueActionTextName].text = "ACTION QUEUE";
		slidersTurnQueue.RemoveRange (0, slidersTurnQueue.Count);
		slidersActionQueue.RemoveRange (0, slidersActionQueue.Count);
	}

	//Resets one slider
	void ResetSliderValue(string name){
		sliders[name].value = 0;
		Image img = (Image)sliders[name].fillRect.transform.GetComponent("Image");
		img.color = sliderColor;
	}

	void IncreaseSliderValue(Slider sld, int speed){
		if (!OneSliderTo100 ()) {
			speed = speed * 10; // Increase factor used when no one is at 100% to minimize idle times
		}
		if (sld.value != 100.0f) { // If the slider passed as parameter isnt full yet
			if (sld.value + Time.deltaTime * speed > 100) { 
				//If this iteration fills the slider, set value to 100, change color and add the fighter to the turn queue
				sld.value = 100.0f; 
				Image img = (Image)sld.fillRect.transform.GetComponent ("Image");
				img.color = Color.green;
				slidersTurnQueue.Add (sld.name);
			}
			else {
				// If it's not full yet and the fighter isnt currently performing an action
				if (!slidersActionQueue.Contains(sld.name)){
					sld.value += Time.deltaTime * speed; //Increase slider value 
				}
			}
		}

	}
}
