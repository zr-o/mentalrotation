using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.IO;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(InputActions))]
public class ActionPerformer : MonoBehaviour
{
    public TMP_Text mainTimer;
    public TMP_Text mainTimer2;
    public TMP_Text responseTimer;
    public TMP_Text responseTimer2;
    private ButtonUI buttonChecker;
    private Canvas canvasObject;
    private Canvas rotControls;
    private GameObject eventSystem;
    private bool participantSelected = false;
    private InputActions _inputData;
    private GameObject decisionMenu;
    private GameObject sameText;
    private GameObject mirrorText;
    private GameObject blockText;
    private GameObject completeText;
    private GameObject rot90, rot180;

    private bool rightHandLastState = false;
    private bool leftHandLastState = false;
    private bool rightButtonValue;
    private bool leftButtonValue;

    private int thisAngle;
    Quaternion transformQuat = new Quaternion();
    private bool rotButtonPressed = false;


    private int participantNumber;
    private float timeTaken = 0.0f;
    private GameObject shape1, shape2, shape3, shape4;
    private GameObject shape1Transform, shape2Transform, shape3Transform, shape4Transform;

    private ArrayList trials = new ArrayList();
    private ArrayList currentTrial = new ArrayList();

    public TMP_Text pathText;
    private bool blockCompleteBool = false;

    public bool menuOpen = false;
    private int menuType = 0;
    private int fixedAngle;
    private int ivCount = 0;
    private int currentIV;
    private int shapeCount, currentShape = 0;
    private bool firstShapeGenerated = false;
    
    //
    public float timeout = 15.0f;
    public bool mainTimerPaused = false;
    public float responseTimeout = 0.0f;

    private ArrayList ivArray = new ArrayList() { "N", "Q", "H"};
    private ArrayList shapeArray = new ArrayList();
    private bool tutComplete = false;
    private bool gameStarted = false;
    private bool shapesDisplayed = false;

    private ArrayList trialLog = new ArrayList();
    private ArrayList shapeLog = new ArrayList();
    private ArrayList difficultyLog = new ArrayList();
    private ArrayList timeTakenLog = new ArrayList();
    private ArrayList answerLog = new ArrayList();
    private ArrayList shapeCountLog = new ArrayList();
    private ArrayList ivLog = new ArrayList();
    private ArrayList quatTheta = new ArrayList();
    private bool logsWritten = false;
    private ArrayList eyeStrings = new ArrayList();
    string currentEyeString = "";

    private void getTrials()
    {
        string thisPath = Path.Combine(Application.persistentDataPath + "//TrialRuns.txt");
        StreamReader reader = new StreamReader(thisPath);
        ArrayList rawTrials = new ArrayList();
        ArrayList splitTrials = new ArrayList();
        while(!reader.EndOfStream)
        {
            string trial = reader.ReadLine();
            rawTrials.Add(trial);
        }
        reader.Close();

        splitTrials.Add(rawTrials[participantNumber]);
        string[] split = splitTrials[0].ToString().Split(",");
        foreach (string item in split)
            trials.Add(item);

        string trials1 = "";
        foreach (var item in trials)
            trials1 = trials1 + item + ",";
        Debug.Log(trials1);
    }

    // Start is called before the first frame update
    void Start()
    {
        _inputData = GetComponent<InputActions>();
        
        //string trials = "";
        //foreach (var item in difficulty)
        //    trials = trials + item + ",";
        //Debug.Log(trials);
        canvasObject = GameObject.Find("Canvas").GetComponent<Canvas>();
        rotControls = GameObject.Find("Canvas2").GetComponent<Canvas>();
        rot90 = GameObject.Find("Rot90");
        rot180 = GameObject.Find("Rot180");
        eventSystem = GameObject.Find("EventSystem");
        decisionMenu = GameObject.Find("ConfirmText");
        sameText = GameObject.Find("SameText");
        mirrorText = GameObject.Find("MirrorText");
        blockText = GameObject.Find("BlockText");
        completeText = GameObject.Find("CompleteText");
        GameObject.Find("Canvas2").GetComponent<GraphicRaycaster>().enabled = false;
        rot90.SetActive(false);
        rot180.SetActive(false);
        
        shape1 = GameObject.Find("Shape1");
        shape2 = GameObject.Find("Shape2");
        shape3 = GameObject.Find("Shape3");
        shape4 = GameObject.Find("Shape4");
        shape1Transform = GameObject.Find("Shape1Transform");
        shape2Transform = GameObject.Find("Shape2Transform");
        shape3Transform = GameObject.Find("Shape3Transform");
        shape4Transform = GameObject.Find("Shape4Transform");
        hideAllShapes();
        decisionMenu.GetComponent<MeshRenderer>().enabled = false;
        sameText.GetComponent<MeshRenderer>().enabled = false;
        mirrorText.GetComponent<MeshRenderer>().enabled = false;
        blockText.GetComponent<MeshRenderer>().enabled = false;
        completeText.GetComponent<MeshRenderer>().enabled = false;
        mainTimer.GetComponent<MeshRenderer>().enabled = true;
        mainTimer2.GetComponent<MeshRenderer>().enabled = true;
        responseTimer.GetComponent<MeshRenderer>().enabled = false;
        responseTimer2.GetComponent<MeshRenderer>().enabled = false;
        responseTimeout = 0.0f;
    }

    void hideAllShapes()
    {
        shape1.GetComponent<MeshRenderer>().enabled = false;
        shape1Transform.GetComponent<MeshRenderer>().enabled = false;
        shape2.GetComponent<MeshRenderer>().enabled = false;
        shape2Transform.GetComponent<MeshRenderer>().enabled = false;
        shape3.GetComponent<MeshRenderer>().enabled = false;
        shape3Transform.GetComponent<MeshRenderer>().enabled = false;
        shape4.GetComponent<MeshRenderer>().enabled = false;
        shape4Transform.GetComponent<MeshRenderer>().enabled = false;
        shapesDisplayed = false;
    }

    
    void menuControl()
    {
        firstShapeGenerated = true;
        if (leftButtonValue != leftHandLastState && menuType == 1)
        {
            if (leftButtonValue == true)
            {
                
            }
            else// if(buttonValue == false)
            {
                string thisTrial = trials[shapeCount].ToString();
                if (thisTrial.ToCharArray()[1] == 'S')
                    answerLog.Add("Y");
                else
                    answerLog.Add("N");

                timeTakenLog.Add(15.0f - timeout);
                timeout = 15.0f;
                responseTimeout = 0.0f;
                
                decisionMenu.GetComponent<MeshRenderer>().enabled = false;
                sameText.GetComponent<MeshRenderer>().enabled = false;
                menuOpen = false;
                rotButtonPressed = false;
                eventSystem.GetComponent<ButtonUI>().buttonBool = false;
                hideShape();
                shapeCount += 1;
                eyeStrings.Add(currentEyeString);
                currentEyeString = "";
                if (shapeCount == 32)
                {

                }
                else
                {
                    newShapeDisplay();
                }
                
            }
            leftHandLastState = leftButtonValue;
        }

        if (rightButtonValue != rightHandLastState && menuType == 2)
        {
            if (rightButtonValue == true)
            {

            }
            else// if(buttonValue == false)
            {
                string thisTrial = trials[shapeCount].ToString();
                if (thisTrial.ToCharArray()[1] == 'M')
                    answerLog.Add("Y");
                else
                    answerLog.Add("N");

                timeTakenLog.Add(15.0f - timeout);
                timeout = 15.0f;
                responseTimeout = 0.0f;
                decisionMenu.GetComponent<MeshRenderer>().enabled = false;
                mirrorText.GetComponent<MeshRenderer>().enabled = false;
                menuOpen = false;
                rotButtonPressed = false;
                eventSystem.GetComponent<ButtonUI>().buttonBool = false;
                hideShape();
                shapeCount += 1;
                eyeStrings.Add(currentEyeString);
                currentEyeString = "";
                if (shapeCount == 32)
                {

                }
                else
                {
                    newShapeDisplay();
                }
            }
            // Set last known state for button
            rightHandLastState = rightButtonValue;
        }
    }

    void tutMenuControl()
    {
        firstShapeGenerated = true;
        if (leftButtonValue != leftHandLastState && menuType == 1)
        {
            if (leftButtonValue == true)
            {
                
            }
            else// if(buttonValue == false)
            {
                timeout = 15.0f;
                responseTimeout = 0.0f;
                
                decisionMenu.GetComponent<MeshRenderer>().enabled = false;
                sameText.GetComponent<MeshRenderer>().enabled = false;
                menuOpen = false;
                rotButtonPressed = false;
                eventSystem.GetComponent<ButtonUI>().buttonBool = false;
                hideShape();
                shapeCount += 1;
                if (shapeCount == 24)
                {
                    tutComplete = true;
                }
                else
                {
                    newShapeDisplay();
                }
            }
            leftHandLastState = leftButtonValue;
        }

        if (rightButtonValue != rightHandLastState && menuType == 2)
        {
            if (rightButtonValue == true)
            {
                
            }
            else// if(buttonValue == false)
            {
                timeout = 15.0f;
                responseTimeout = 0.0f;
                
                decisionMenu.GetComponent<MeshRenderer>().enabled = false;
                mirrorText.GetComponent<MeshRenderer>().enabled = false;
                menuOpen = false;
                rotButtonPressed = false;
                eventSystem.GetComponent<ButtonUI>().buttonBool = false;
                hideShape();
                shapeCount += 1;
                if (shapeCount == 24)
                {
                    tutComplete = true;
                }
                else
                {
                    newShapeDisplay();
                }
                
            }
            // Set last known state for button
            rightHandLastState = rightButtonValue;
        }
    }

    void sameShapeDisplay()
    {
        shapesDisplayed = true;
        string thisTrial = trials[shapeCount].ToString();
        switch (thisTrial.ToCharArray()[2])
        {
            case 'I':
                shape1.GetComponent<MeshRenderer>().enabled = true;
                shape1Transform.GetComponent<MeshRenderer>().enabled = true;
                break;
            case 'J':
                shape2.GetComponent<MeshRenderer>().enabled = true;
                shape2Transform.GetComponent<MeshRenderer>().enabled = true;
                break;
            case 'K':
                shape3.GetComponent<MeshRenderer>().enabled = true;
                shape3Transform.GetComponent<MeshRenderer>().enabled = true;
                break;
            case 'L':
                shape4.GetComponent<MeshRenderer>().enabled = true;
                shape4Transform.GetComponent<MeshRenderer>().enabled = true;
                break;
        }
    }

    void newShapeDisplay()
    {
        if(tutComplete)
        {
            trialLog.Add(trials[shapeCount].ToString());
            ivLog.Add(currentIV);
        }
            
        
        rotButtonPressed = false;
        shapesDisplayed = true;
        responseTimeout = 0.0f;

        int thisRot = 0;
        //Debug.Log("Shape count: " + shapeCount);
        string thisTrial = trials[shapeCount].ToString();

        if (thisTrial.ToCharArray()[0] == 'E')
            thisRot = easyRotGen();
        else
            thisRot = hardRotGen();

        difficultyLog.Add(thisRot);
        //Debug.Log("Trial: " + trials[shapeCount].ToString());
        //Debug.Log("Rotating by: " + thisRot);

        int[] randRot = { 0, 0, 0 };

        for (int i = 0; i < 3; i++)
            randRot[i] = Random.Range(0, 181);

        fixedAngle = Random.Range(0, 3);
        thisAngle = fixedAngle;
        Quaternion initQuat = Quaternion.Euler(randRot[0], randRot[1], randRot[2]);
        switch (fixedAngle)
        {
            case 0:
                transformQuat = initQuat * Quaternion.Euler(thisRot, 0, 0);
                break;
            case 1:
                transformQuat = initQuat * Quaternion.Euler(0, thisRot, 0);
                break;
            case 2:
                transformQuat = initQuat * Quaternion.Euler(0, 0, thisRot);
                break;
        }

        if (tutComplete)
            quatTheta.Add(transformQuat[3]);

        if (thisTrial.ToCharArray()[1] == 'S')
        {
            // Rot shape 1 then rot shape 2 as a product of shape 1
            switch (thisTrial.ToCharArray()[2])
            {
                case 'I':
                    shapeLog.Add(1);
                    shape1.transform.localRotation = initQuat;
                    shape1Transform.transform.localRotation = transformQuat;
                    shape1.GetComponent<MeshRenderer>().enabled = true;
                    shape1Transform.GetComponent<MeshRenderer>().enabled = true;
                    currentShape = 1;
                    break;
                case 'J':
                    shapeLog.Add(2);
                    shape2.transform.localRotation = initQuat;
                    shape2Transform.transform.localRotation = transformQuat;
                    shape2.GetComponent<MeshRenderer>().enabled = true;
                    shape2Transform.GetComponent<MeshRenderer>().enabled = true;
                    currentShape = 2;
                    break;
                case 'K':
                    shapeLog.Add(3);
                    shape3.transform.localRotation = initQuat;
                    shape3Transform.transform.localRotation = transformQuat;
                    shape3.GetComponent<MeshRenderer>().enabled = true;
                    shape3Transform.GetComponent<MeshRenderer>().enabled = true;
                    currentShape = 3;
                    break;
                case 'L':
                    shapeLog.Add(4);
                    shape4.transform.localRotation = initQuat;
                    shape4Transform.transform.localRotation = transformQuat;
                    shape4.GetComponent<MeshRenderer>().enabled = true;
                    shape4Transform.GetComponent<MeshRenderer>().enabled = true;
                    currentShape = 4;
                    break;
            }
        }
        else
        {
            switch (thisTrial.ToCharArray()[2])
            {
                case 'I':
                    shapeLog.Add(1);
                    shape1Transform.transform.localScale = -shape1Transform.transform.localScale;
                    //shape1Transform.transform.position = new Vector3(1.362f, 1.128f, 2.423f);
                    shape1.transform.localRotation = initQuat;
                    shape1Transform.transform.localRotation = transformQuat;
                    shape1.GetComponent<MeshRenderer>().enabled = true;
                    shape1Transform.GetComponent<MeshRenderer>().enabled = true;
                    currentShape = 1;
                    break;
                case 'J':
                    shapeLog.Add(2);
                    shape2Transform.transform.localScale = -shape2Transform.transform.localScale;
                    //shape2Transform.transform.position = new Vector3(1.522f, 0.9679f, 2.902f);
                    shape2.transform.localRotation = initQuat;
                    shape2Transform.transform.localRotation = transformQuat;
                    shape2.GetComponent<MeshRenderer>().enabled = true;
                    shape2Transform.GetComponent<MeshRenderer>().enabled = true;
                    currentShape = 2;
                    break;
                case 'K':
                    shapeLog.Add(3);
                    shape3Transform.transform.localScale = -shape3Transform.transform.localScale;
                    //shape3Transform.transform.position = new Vector3(1.523f, 1.129f, 2.262f);
                    shape3.transform.localRotation = initQuat;
                    shape3Transform.transform.localRotation = transformQuat;
                    shape3.GetComponent<MeshRenderer>().enabled = true;
                    shape3Transform.GetComponent<MeshRenderer>().enabled = true;
                    currentShape = 3;
                    break;
                case 'L':
                    shapeLog.Add(4);
                    shape4Transform.transform.localScale = -shape4Transform.transform.localScale;
                    //shape4Transform.transform.position = new Vector3(1.682f, 1.128f, 2.581f);
                    shape4.transform.localRotation = initQuat;
                    shape4Transform.transform.localRotation = transformQuat;
                    shape4.GetComponent<MeshRenderer>().enabled = true;
                    shape4Transform.GetComponent<MeshRenderer>().enabled = true;
                    currentShape = 4;
                    break;
            }
        }
        
    }

    void hideShape()
    {
        switch(currentShape)
        {
            case 1:
                shape1.GetComponent<MeshRenderer>().enabled = false;
                shape1Transform.GetComponent<MeshRenderer>().enabled = false;
                break;
            case 2:
                shape2.GetComponent<MeshRenderer>().enabled = false;
                shape2Transform.GetComponent<MeshRenderer>().enabled = false;
                break;
            case 3:
                shape3.GetComponent<MeshRenderer>().enabled = false;
                shape3Transform.GetComponent<MeshRenderer>().enabled = false;
                break;
            case 4:
                shape4.GetComponent<MeshRenderer>().enabled = false;
                shape4Transform.GetComponent<MeshRenderer>().enabled = false;
                break;
        }

        shape1Transform.transform.localScale = new Vector3(8, 8, 8);
        shape2Transform.transform.localScale = new Vector3(8, 8, 8);
        shape3Transform.transform.localScale = new Vector3(8, 8, 8);
        shape4Transform.transform.localScale = new Vector3(8, 8, 8);
        shapesDisplayed = false;
    }

    void displayMenus()
    {
        decisionMenu.GetComponent<MeshRenderer>().enabled = true;

        if (menuType == 1)
            sameText.GetComponent<MeshRenderer>().enabled = true;
        else if (menuType == 2)
            mirrorText.GetComponent<MeshRenderer>().enabled = true;
    }

    private int easyRotGen()
    {
        return Random.Range(45, 61);
    }

    private int hardRotGen()
    {
        return Random.Range(120, 136);
    }

    private int getNewIV()
    {
        Debug.Log("Current IV: " + currentIV);
        int ivRand = Random.Range(0, ivArray.Count);
        switch (ivArray[ivRand].ToString())
        {
            case "N":
                ivArray.Remove("N");
                return 0;
            case "Q":
                ivArray.Remove("Q");
                return 1;
            case "H":
                ivArray.Remove("H");
                return 2;
        }
        return 3;
    }

    public void trialsComplete()
    {
        completeText.GetComponent<MeshRenderer>().enabled = true;
    }

    public void blockComplete()
    {
        blockText.GetComponent<MeshRenderer>().enabled = true;
        blockCompleteBool = true;
    }
    
    void timeoutTrial()
    {
        hideShape();
        shapeCount += 1;
        rotButtonPressed = false;
        newShapeDisplay();
        timeout = 15.0f;
        responseTimeout = 0.0f;
        timeTaken = 0;
        menuOpen = false;
    }

    void timeoutResponse()
    {
        timeout -= timeTaken;
        timeTaken = 0.0f;
    }


    void userInputRot()
    {
        if (currentIV == 1)
        {
            Debug.Log("Rot90 switch");

            switch (thisAngle)
            {
                case 0:
                    transformQuat = transformQuat * Quaternion.Euler(90, 0, 0);
                    break;
                case 1:
                    transformQuat = transformQuat * Quaternion.Euler(0, 90, 0);
                    break;
                case 2:
                    transformQuat = transformQuat * Quaternion.Euler(0, 0, 90);
                    break;

            }
        }
        else if (currentIV == 2)
        {
            Debug.Log("Rot180 switch");
            switch (thisAngle)
            {
                case 0:
                    transformQuat = transformQuat * Quaternion.Euler(180, 0, 0);
                    break;
                case 1:
                    transformQuat = transformQuat * Quaternion.Euler(0, 180, 0);
                    break;
                case 2:
                    transformQuat = transformQuat * Quaternion.Euler(0, 0, 180);
                    break;
            }
        }


        switch(currentShape)
        {
            
            
            case 1:
                Debug.Log("Adding rotation1");
                shape1Transform.transform.localRotation = transformQuat;
                break;
            case 2:
                Debug.Log("Adding rotation2");
                shape2Transform.transform.localRotation = transformQuat;
                break;
            case 3:
                Debug.Log("Adding rotation3");
                shape3Transform.transform.localRotation = transformQuat;
                break;
            case 4:
                Debug.Log("Adding rotation4");
                shape4Transform.transform.localRotation = transformQuat;
                break;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 eyeData;
        _inputData._HMD.TryGetFeatureValue(CommonUsages.centerEyePosition, out eyeData);
        if (tutComplete && gameStarted && firstShapeGenerated)
            currentEyeString = currentEyeString + "\t" + eyeData;
        _inputData._rightController.TryGetFeatureValue(CommonUsages.primaryButton, out rightButtonValue);
        _inputData._leftController.TryGetFeatureValue(CommonUsages.primaryButton, out leftButtonValue);

        if (eventSystem.GetComponent<ButtonUI>().buttonBool && !participantSelected)
        {
            getTrials();
            canvasObject.enabled = !canvasObject.enabled;
            participantSelected = true;
            GameObject.Find("Canvas2").GetComponent<GraphicRaycaster>().enabled = true;
            string resultString = Regex.Match(eventSystem.GetComponent<ButtonUI>().buttonPressed, @"\d+").Value;
            participantNumber = int.Parse(resultString);
            eventSystem.GetComponent<ButtonUI>().buttonBool = false;
        }
        
        

        if (!tutComplete && participantSelected && shapeCount < 24)
        {
            if (eventSystem.GetComponent<ButtonUI>().buttonBool && participantSelected && !rotButtonPressed)
            {
                Debug.Log("Running user rot");
                userInputRot();
                rotButtonPressed = true;
                eventSystem.GetComponent<ButtonUI>().buttonBool = false;
            }
            if (!mainTimerPaused)
            {
                timeout -= Time.deltaTime;
                mainTimer.text = ((int)timeout % 60).ToString();
                mainTimer2.text = ((int)timeout % 60).ToString();
                if (timeout <= 0.0f)
                    timeoutTrial();
            }

            if (!shapesDisplayed && firstShapeGenerated == false)
                newShapeDisplay();
            if (shapeCount == 8)
                currentIV = 1;
            else if (shapeCount == 16)
                currentIV = 2;

            if (currentIV == 0)
            {
                rot90.SetActive(false);
                rot180.SetActive(false);
            }
            else if (currentIV == 1)
            {
                rot90.SetActive(true);
                rot180.SetActive(false);
            }
            else if (currentIV == 2)
            {
                rot90.SetActive(false);
                rot180.SetActive(true);
            }

            if (_inputData._leftController.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerValue) && leftTriggerValue && !menuOpen)
            {
                menuOpen = true;
                mainTimerPaused = true;
                menuType = 1;
                mainTimer.GetComponent<MeshRenderer>().enabled = false;
                mainTimer2.GetComponent<MeshRenderer>().enabled = false;
                responseTimer.GetComponent<MeshRenderer>().enabled = true;
                responseTimer2.GetComponent<MeshRenderer>().enabled = true;
                displayMenus();
                
            }
            if (_inputData._rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerValue) && rightTriggerValue && !menuOpen)
            {
                menuOpen = true;
                mainTimerPaused = true;
                menuType = 2;
                mainTimer.GetComponent<MeshRenderer>().enabled = false;
                mainTimer2.GetComponent<MeshRenderer>().enabled = false;
                responseTimer.GetComponent<MeshRenderer>().enabled = true;
                responseTimer2.GetComponent<MeshRenderer>().enabled = true;
                displayMenus();
            }
            if (menuOpen == true)
            {
                responseTimeout += Time.deltaTime;
                responseTimer.text = ((int)responseTimeout % 60).ToString();
                responseTimer2.text = ((int)responseTimeout % 60).ToString();
                hideAllShapes();
                timeTaken += Time.deltaTime;
                if (timeTaken >= 3.0f)
                {
                    timeoutResponse();
                }
                if (_inputData._leftController.TryGetFeatureValue(CommonUsages.triggerButton, out bool left2TriggerValue) && !left2TriggerValue && menuType == 1)
                {
                    sameShapeDisplay();
                    mainTimerPaused = false;
                    mainTimer.GetComponent<MeshRenderer>().enabled = true;
                    mainTimer2.GetComponent<MeshRenderer>().enabled = true;
                    responseTimer.GetComponent<MeshRenderer>().enabled = false;
                    responseTimer2.GetComponent<MeshRenderer>().enabled = false;
                    menuOpen = false;
                    decisionMenu.GetComponent<MeshRenderer>().enabled = false;
                    sameText.GetComponent<MeshRenderer>().enabled = false;
                }
                if(_inputData._rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool right2TriggerValue) && !right2TriggerValue  && menuType == 2)
                {
                    sameShapeDisplay();
                    mainTimerPaused = false;
                    mainTimer.GetComponent<MeshRenderer>().enabled = true;
                    mainTimer2.GetComponent<MeshRenderer>().enabled = true;
                    responseTimer.GetComponent<MeshRenderer>().enabled = false;
                    responseTimer2.GetComponent<MeshRenderer>().enabled = false;
                    menuOpen = false;
                    decisionMenu.GetComponent<MeshRenderer>().enabled = false;
                    mirrorText.GetComponent<MeshRenderer>().enabled = false;
                }

                tutMenuControl();
                
            }
        }
        else if (gameStarted && shapeCount < 32 && participantSelected && !blockCompleteBool)
        {
            if (!mainTimerPaused)
            {
                timeout -= Time.deltaTime;
                mainTimer.text = ((int)timeout % 60).ToString();
                mainTimer2.text = ((int)timeout % 60).ToString();
                if (timeout <= 0.0f)
                    timeoutTrial();
            }
            if (!shapesDisplayed && firstShapeGenerated == false)
                newShapeDisplay();

            if (currentIV == 0)
            {
                rot90.SetActive(false);
                rot180.SetActive(false);
            }
            else if (currentIV == 1)
            {
                rot90.SetActive(true);
                rot180.SetActive(false);
            }
            else if (currentIV == 2)
            {
                rot90.SetActive(false);
                rot180.SetActive(true);
            }
            //Debug.Log("ButtonBool: " + eventSystem.GetComponent<ButtonUI>().buttonBool);
            //Debug.Log("RotPressed: " + rotButtonPressed);
            if (eventSystem.GetComponent<ButtonUI>().buttonBool && participantSelected && !rotButtonPressed)
            {
                Debug.Log("Running user rot");
                userInputRot();
                rotButtonPressed = true;
                eventSystem.GetComponent<ButtonUI>().buttonBool = false;
            }

            if (_inputData._leftController.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerValue) && leftTriggerValue && !menuOpen)
            {
                mainTimerPaused = true;
                mainTimer.GetComponent<MeshRenderer>().enabled = false;
                mainTimer2.GetComponent<MeshRenderer>().enabled = false;
                responseTimer.GetComponent<MeshRenderer>().enabled = true;
                responseTimer2.GetComponent<MeshRenderer>().enabled = true;
                menuOpen = true;
                menuType = 1;
                displayMenus();
            }
            if (_inputData._rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerValue) && rightTriggerValue && !menuOpen)
            {
                mainTimerPaused = true;
                mainTimer.GetComponent<MeshRenderer>().enabled = false;
                mainTimer2.GetComponent<MeshRenderer>().enabled = false;
                responseTimer.GetComponent<MeshRenderer>().enabled = true;
                responseTimer2.GetComponent<MeshRenderer>().enabled = true;
                menuOpen = true;
                menuType = 2;
                displayMenus();
            }
            if (menuOpen == true)
            {
                responseTimeout += Time.deltaTime;
                responseTimer.text = ((int)responseTimeout % 60).ToString();
                responseTimer2.text = ((int)responseTimeout % 60).ToString();
                hideAllShapes();
                timeTaken += Time.deltaTime;
                if (timeTaken >= 3.0f)
                {
                    timeoutResponse();
                }
                if (_inputData._leftController.TryGetFeatureValue(CommonUsages.triggerButton, out bool left2TriggerValue) && !left2TriggerValue && menuType == 1)
                {
                    sameShapeDisplay();
                    mainTimerPaused = false;
                    mainTimer.GetComponent<MeshRenderer>().enabled = true;
                    mainTimer2.GetComponent<MeshRenderer>().enabled = true;
                    responseTimer.GetComponent<MeshRenderer>().enabled = false;
                    responseTimer2.GetComponent<MeshRenderer>().enabled = false;
                    //Debug.Log("Left Trigger Pressed");
                    menuOpen = false;
                    decisionMenu.GetComponent<MeshRenderer>().enabled = false;
                    sameText.GetComponent<MeshRenderer>().enabled = false;
                }
                if (_inputData._rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool right2TriggerValue) && !right2TriggerValue && menuType == 2)
                {
                    sameShapeDisplay();
                    mainTimerPaused = false;
                    mainTimer.GetComponent<MeshRenderer>().enabled = true;
                    mainTimer2.GetComponent<MeshRenderer>().enabled = true;
                    responseTimer.GetComponent<MeshRenderer>().enabled = false;
                    responseTimer2.GetComponent<MeshRenderer>().enabled = false;
                    //Debug.Log("Right Trigger Pressed");
                    menuOpen = false;
                    decisionMenu.GetComponent<MeshRenderer>().enabled = false;
                    mirrorText.GetComponent<MeshRenderer>().enabled = false;
                }
                menuControl();
            }
        }
        else if (participantSelected)
        {
            hideAllShapes();

            rot90.SetActive(false);
            rot180.SetActive(false);


            if (ivCount < 3)
            {
                blockComplete();
                gameStarted = false;
                if (_inputData._rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool buttonBPressed) && buttonBPressed)
                {
                    Debug.Log("Block complete");
                    blockText.GetComponent<MeshRenderer>().enabled = false;
                    gameStarted = true;
                    firstShapeGenerated = false;
                    mainTimerPaused = false;
                    mainTimer.GetComponent<MeshRenderer>().enabled = true;
                    mainTimer2.GetComponent<MeshRenderer>().enabled = true;
                    responseTimer.GetComponent<MeshRenderer>().enabled = false;
                    responseTimer2.GetComponent<MeshRenderer>().enabled = false;
                    shapeCount = 0;
                    timeout = 15.0f;
                    currentIV = getNewIV();
                    Debug.Log("New IV: " + currentIV);
                    ivCount += 1;
                    blockCompleteBool = false;
                }
            }
            else
            {
                
                if (!logsWritten)
                {
                    trialsComplete();
                    Debug.Log("Trials Complete");
                    logsWritten = true;

                    string thisPath = Path.Combine(Application.persistentDataPath + "//Participant " + participantNumber + ".txt");
                    string eyeLogPath = Path.Combine(Application.persistentDataPath + "//Participant " + participantNumber + " Eyes.txt");
                    string writeEyeLog = "";
                    string writeTrialsLog = "";
                    for (int i = 0; i < 96; i++)
                    {
                        writeTrialsLog = writeTrialsLog + trialLog[i].ToString() + "\t";
                        writeTrialsLog = writeTrialsLog + ivLog[i].ToString() + "\t";
                        writeTrialsLog = writeTrialsLog + shapeLog[i].ToString() + "\t";
                        writeTrialsLog = writeTrialsLog + difficultyLog[i].ToString() + "\t";
                        writeTrialsLog = writeTrialsLog + timeTakenLog[i].ToString() + "\t";
                        writeTrialsLog = writeTrialsLog + answerLog[i].ToString() + "\t";
                        writeTrialsLog = writeTrialsLog + quatTheta[i].ToString() + "\t";
                        writeTrialsLog = writeTrialsLog + "\n";
                    }
                    for (int i = 0; i < 96; i++)
                    {
                        Debug.Log(eyeStrings[i].ToString());
                        writeEyeLog = writeEyeLog + eyeStrings[i].ToString() + "\n";
                    }
                    
                    File.AppendAllText(thisPath, writeTrialsLog.ToString());
                    File.AppendAllText(eyeLogPath, writeEyeLog.ToString());
                }
                
                // Add trials finished message
                // Add button to dump logs
            }
        }
    }
}
