using System.Collections.Generic;
using UnityEngine;

namespace Script
{
    public class CorridorPathingDecider
    {
        private const int CORRIDOR_END = GeneratorGlobalVals.CORRIDOR_END;
        private const int STRAIGHT = GeneratorGlobalVals.STRAIGHT;
        private const int LEFT_TURN = GeneratorGlobalVals.LEFT_TURN;
        private const int RIGHT_TURN = GeneratorGlobalVals.RIGHT_TURN;
        private const int LEFT_UP_SPLIT = GeneratorGlobalVals.LEFT_UP_SPLIT;
        private const int RIGHT_UP_SPLIT = GeneratorGlobalVals.RIGHT_UP_SPLIT;
        private const int LEFT_RIGHT_SPLIT = GeneratorGlobalVals.LEFT_RIGHT_SPLIT;
        private const int LEFT_UP_RIGHT_SPLIT = GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT;
        private const int CORNER = 10;
        private const int TWO_WAY_SPLIT = 11;
        private float _cornerChance;
        private readonly float[] _cornerChanceBounds;
        private float _straightChance;

        //Decides what corridor piece will be added next, based off what can and cannot be placed and their respective weights
        private readonly float[] _straightChanceBounds;
        private float _threeWaySplitChance;
        private readonly float[] _threeWaySplitChanceBounds;
        private float _twoWaySplitChance;
        private readonly float[] _twoWaySplitChanceBounds;


        public CorridorPathingDecider()
        {
            _straightChanceBounds = GeneratorGlobalVals.Instance.GetStraightChance();
            _cornerChanceBounds = GeneratorGlobalVals.Instance.GetCornerChance();
            _twoWaySplitChanceBounds = GeneratorGlobalVals.Instance.GetTwoWayChance();
            _threeWaySplitChanceBounds = GeneratorGlobalVals.Instance.GetThreeWayChance();
        }

        private void SetChances()
        {
            _straightChance = Random.Range(_straightChanceBounds[0], _straightChanceBounds[1]);
            _cornerChance = Random.Range(_cornerChanceBounds[0], _cornerChanceBounds[1]);
            _twoWaySplitChance = Random.Range(_twoWaySplitChanceBounds[0], _twoWaySplitChanceBounds[1]);
            _threeWaySplitChance = Random.Range(_threeWaySplitChanceBounds[0], _threeWaySplitChanceBounds[1]);
        }

        public int DecideNextCorridor(bool isStraightValid, bool isLeftValid, bool isRightValid)
        {
            SetChances();
            var options = new List<KeyValuePair<int, float>>();
            var validCorners = new List<int>();
            var validTwoWaySplits = new List<int>();
            var total = CalcValidOptions(ref options, ref validCorners, ref validTwoWaySplits, isStraightValid,
                isLeftValid, isRightValid);

            if (total == 0f) return CORRIDOR_END;

            var chosenNum = Random.Range(0f, total);
            var chosenOptionFound = false;
            var chosenOption = CORRIDOR_END; //Default value
            float currentTotal = 0;
            for (var i = 0; i < options.Count && !chosenOptionFound; i++)
            {
                currentTotal += options[i].Value;
                if (currentTotal >= chosenNum)
                {
                    chosenOption = options[i].Key;
                    chosenOptionFound = true;
                }
            }

            switch (chosenOption)
            {
                case STRAIGHT:
                    return STRAIGHT;
                case LEFT_UP_RIGHT_SPLIT:
                    return LEFT_UP_RIGHT_SPLIT;
                case CORNER:
                    var chosenCorner = Random.Range(0, validCorners.Count);
                    return validCorners[chosenCorner];
                case TWO_WAY_SPLIT:
                    var chosenSpilt = Random.Range(0, validTwoWaySplits.Count);
                    return validTwoWaySplits[chosenSpilt];
            }

            return CORRIDOR_END;
        }

        public int DecideNextCorridorOLD(bool isStraightValid, bool isLeftValid, bool isRightValid)
        {
            var options = new List<KeyValuePair<int, float>>();
            var validCorners = new List<int>();
            var validTwoWaySplits = new List<int>();
            var total = CalcValidOptions(ref options, ref validCorners, ref validTwoWaySplits, isStraightValid,
                isLeftValid, isRightValid);

            if (total == 0f) return CORRIDOR_END;

            if (options.Count == 1) return options[0].Key;
            var chosenNum = Random.Range(0f, total);
            var chosenOptionFound = false;
            var chosenOption = CORRIDOR_END; //Default value
            float currentTotal = 0;
            for (var i = 0; i < options.Count && !chosenOptionFound; i++)
            {
                currentTotal += options[i].Value;
                if (currentTotal >= chosenNum)
                {
                    chosenOption = options[i].Key;
                    chosenOptionFound = true;
                }
            }

            return chosenOption;
        }

        /**
         * Works out what types of corridor segments are valid.
         * Adds valid options to the list, options (passed by ref)
         * Adds total of valid options, returns this total (and options)
         */
        private float CalcValidOptions(ref List<KeyValuePair<int, float>> options, ref List<int> validCorners,
            ref List<int> validTwoWaySplits, bool isStraightValid, bool isLeftValid, bool isRightValid)
        {
            var cornerAdded = false;
            var twoWaySplitAdded = false;
            float total = 0;
            if (isStraightValid)
            {
                var option = new KeyValuePair<int, float>(STRAIGHT, _straightChance);
                options.Add(option);
                total += _straightChance;
            }

            if (isLeftValid)
            {
                validCorners.Add(LEFT_TURN);
                var option = new KeyValuePair<int, float>(CORNER, _cornerChance);
                options.Add(option);
                total += _cornerChance;
                cornerAdded = true;
            }

            if (isRightValid)
            {
                validCorners.Add(RIGHT_TURN);
                if (!cornerAdded)
                {
                    var option = new KeyValuePair<int, float>(CORNER, _cornerChance);
                    options.Add(option);
                    total += _cornerChance;
                }
            }

            if (isStraightValid && isLeftValid)
            {
                validTwoWaySplits.Add(LEFT_UP_SPLIT);
                var option = new KeyValuePair<int, float>(TWO_WAY_SPLIT, _twoWaySplitChance);
                options.Add(option);
                total += _twoWaySplitChance;
                twoWaySplitAdded = true;
            }

            if (isStraightValid && isRightValid)
            {
                validTwoWaySplits.Add(RIGHT_UP_SPLIT);
                if (!twoWaySplitAdded)
                {
                    var option = new KeyValuePair<int, float>(TWO_WAY_SPLIT, _twoWaySplitChance);
                    options.Add(option);
                    total += _twoWaySplitChance;
                    twoWaySplitAdded = true;
                }
            }

            if (isLeftValid && isRightValid)
            {
                validTwoWaySplits.Add(LEFT_RIGHT_SPLIT);
                if (!twoWaySplitAdded)
                {
                    var option = new KeyValuePair<int, float>(TWO_WAY_SPLIT, _twoWaySplitChance);
                    options.Add(option);
                    total += _twoWaySplitChance;
                }
            }

            if (isLeftValid && isRightValid && isStraightValid)
            {
                var option = new KeyValuePair<int, float>(LEFT_UP_RIGHT_SPLIT, _threeWaySplitChance);
                options.Add(option);
                total += _threeWaySplitChance;
            }

            return total;
        }

        private float CalcValidOptionsOLD(ref List<KeyValuePair<int, float>> options, bool isStraightValid,
            bool isLeftValid, bool isRightValid)
        {
            float total = 0;
            if (isStraightValid)
            {
                var option = new KeyValuePair<int, float>(STRAIGHT, _straightChance);
                options.Add(option);
                total += _straightChance;
            }

            if (isLeftValid)
            {
                var option = new KeyValuePair<int, float>(LEFT_TURN, _cornerChance);
                options.Add(option);
                total += _cornerChance;
            }

            if (isRightValid)
            {
                var option = new KeyValuePair<int, float>(RIGHT_TURN, _cornerChance);
                options.Add(option);
                total += _cornerChance;
            }

            if (isStraightValid && isLeftValid)
            {
                var option = new KeyValuePair<int, float>(LEFT_UP_SPLIT, _twoWaySplitChance);
                options.Add(option);
                total += _twoWaySplitChance;
            }

            if (isStraightValid && isRightValid)
            {
                var option = new KeyValuePair<int, float>(RIGHT_UP_SPLIT, _twoWaySplitChance);
                options.Add(option);
                total += _twoWaySplitChance;
            }

            if (isLeftValid && isRightValid)
            {
                var option = new KeyValuePair<int, float>(LEFT_RIGHT_SPLIT, _twoWaySplitChance);
                options.Add(option);
                total += _twoWaySplitChance;
            }

            if (isLeftValid && isRightValid && isStraightValid)
            {
                var option = new KeyValuePair<int, float>(LEFT_UP_RIGHT_SPLIT, _threeWaySplitChance);
                options.Add(option);
                total += _threeWaySplitChance;
            }

            return total;
        }
    }
}