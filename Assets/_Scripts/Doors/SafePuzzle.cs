using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Istasyon.Interactables
{
    public class SafePuzzle : MonoBehaviour
    {
        [Header("Ring Transforms")]
        public RectTransform outerRing;
        public RectTransform middleRing;
        public RectTransform innerRing;

        [Header("Colors")]
        public Color defaultColor = Color.white;
        public Color correctColor = Color.yellow;
        public Color incorrectColor = Color.red;

        [Header("Puzzle Settings")]
        public int positionsPerRing = 4;
        public int[] correctCombination = new int[3] { 0, 0, 0 }; 

        [Header("Completion")]
        public GameObject safeUIPanel;
        public UnityEvent onPuzzleSolved;

        private int[] _currentPositions = new int[3] { 0, 0, 0 };
        private bool _isSolved = false;

        private void Start()
        {
            ResetColors(); // Start them all white!
        }

        public void SpinOuterLeft() => SpinRing(0, 1);
        public void SpinOuterRight() => SpinRing(0, -1);
        public void SpinMiddleLeft() => SpinRing(1, 1);
        public void SpinMiddleRight() => SpinRing(1, -1);
        public void SpinInnerLeft() => SpinRing(2, 1);
        public void SpinInnerRight() => SpinRing(2, -1);

        private void SpinRing(int ringIndex, int direction)
        {
            if (_isSolved) return; 

            // Update math
            _currentPositions[ringIndex] += direction;
            
            if (_currentPositions[ringIndex] < 0) 
                _currentPositions[ringIndex] = positionsPerRing - 1;
            else if (_currentPositions[ringIndex] >= positionsPerRing) 
                _currentPositions[ringIndex] = 0;

            // Rotate visually
            float anglePerStep = 360f / positionsPerRing; 
            float targetAngle = _currentPositions[ringIndex] * anglePerStep;

            RectTransform targetRing = ringIndex == 0 ? outerRing : (ringIndex == 1 ? middleRing : innerRing);
            targetRing.localEulerAngles = new Vector3(0, 0, targetAngle);

            // Turn everything back to white the moment they spin a ring
            ResetColors();
        }

        private void ResetColors()
        {
            // Find every letter attached to the rings and make them white
            foreach (var txt in outerRing.GetComponentsInChildren<TextMeshProUGUI>()) txt.color = defaultColor;
            foreach (var txt in middleRing.GetComponentsInChildren<TextMeshProUGUI>()) txt.color = defaultColor;
            foreach (var txt in innerRing.GetComponentsInChildren<TextMeshProUGUI>()) txt.color = defaultColor;
        }

        // MAGIC FUNCTION: Physically finds whichever letter is currently at the top of the screen!
        private TextMeshProUGUI GetTopLetter(RectTransform ring)
        {
            TextMeshProUGUI topLetter = null;
            float highestY = float.MinValue;

            foreach (TextMeshProUGUI letter in ring.GetComponentsInChildren<TextMeshProUGUI>())
            {
                // Checks the actual world position. The highest Y value is the top one!
                if (letter.transform.position.y > highestY)
                {
                    highestY = letter.transform.position.y;
                    topLetter = letter;
                }
            }
            return topLetter;
        }

        public void CheckSolution()
        {
            if (_isSolved) return;

            // Math check for the correct combination
            bool outerCorrect = _currentPositions[0] == correctCombination[0];
            bool middleCorrect = _currentPositions[1] == correctCombination[1];
            bool innerCorrect = _currentPositions[2] == correctCombination[2];

            // Grab the exact 3 letters currently sitting at the top
            TextMeshProUGUI currentOuterTop = GetTopLetter(outerRing);
            TextMeshProUGUI currentMiddleTop = GetTopLetter(middleRing);
            TextMeshProUGUI currentInnerTop = GetTopLetter(innerRing);

            // Color those specific top letters Yellow or Red
            if (currentOuterTop != null) currentOuterTop.color = outerCorrect ? correctColor : incorrectColor;
            if (currentMiddleTop != null) currentMiddleTop.color = middleCorrect ? correctColor : incorrectColor;
            if (currentInnerTop != null) currentInnerTop.color = innerCorrect ? correctColor : incorrectColor;

            if (outerCorrect && middleCorrect && innerCorrect)
            {
                Debug.Log("[SafePuzzle] PASSCODE CORRECT! Unlocking...");
                _isSolved = true;
                
                if (safeUIPanel != null) safeUIPanel.SetActive(false);
                onPuzzleSolved?.Invoke();
            }
            else
            {
                Debug.Log("[SafePuzzle] INCORRECT! Try again.");
            }
        }
    }
}