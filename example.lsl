default
{
    state_entry()
    {
        llOwnerSay(llDumpList2String([
            "OS_MATH_PHI: " + (string)OS_MATH_PHI,
            "OS_MATH_TWO_PHI: " + (string)OS_MATH_TWO_PHI,
            "OS_MATH_PHI_BY_TWO: " + (string)OS_MATH_PHI_BY_TWO,
            "OS_MATH_TAU: " + (string)OS_MATH_TAU,
            "OS_MATH_TWO_TAU: " + (string)OS_MATH_TWO_TAU,
            "OS_MATH_TAU_BY_TWO: " + (string)OS_MATH_TAU_BY_TWO,
            "osMathVecMultiply: " + (string)osMathVecMultiply(<1,2,3>, <4,5,6>),
            "osMathVecDivide: " + (string)osMathVecDivide(<1,2,3>, <4,5,6>),
            "osMathVecFloor:" + (string)osMathVecFloor(<0.25, 0.5, 0.75>),
            "osMathVecRound:" + (string)osMathVecRound(<0.25, 0.5, 0.75>),
            "osMathVecCeil:" + (string)osMathVecCeil(<0.25, 0.5, 0.75>)
        ], "\n"));
    }
}
