default
{
    state_entry()
    {
        llOwnerSay(llDumpList2String([
            "MATH_PHI: " + (string)MATH_PHI,
            "MATH_TWO_PHI: " + (string)MATH_TWO_PHI,
            "MATH_PHI_BY_TWO: " + (string)MATH_PHI_BY_TWO,
            "MATH_TAU: " + (string)MATH_TAU,
            "MATH_TWO_TAU: " + (string)MATH_TWO_TAU,
            "MATH_TAU_BY_TWO: " + (string)MATH_TAU_BY_TWO,
            "mathVecMultiply: " + (string)mathVecMultiply(<1,2,3>, <4,5,6>),
            "mathVecDivide: " + (string)mathVecDivide(<1,2,3>, <4,5,6>),
            "mathVecFloor:" + (string)mathVecFloor(<0.25, 0.5, 0.75>),
            "mathVecRound:" + (string)mathVecRound(<0.25, 0.5, 0.75>),
            "mathVecCeil:" + (string)mathVecCeil(<0.25, 0.5, 0.75>),
			"mathVecVolume: " + (string)mathVecVolume(llGetScale())
        ], "\n"));

        llOwnerSay(llList2CSV(mathFibonacci(-10, 20)));
    }
}
