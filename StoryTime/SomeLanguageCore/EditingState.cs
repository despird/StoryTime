using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Used for Editor Autocompletion
namespace SOME.SomeLanguageCore
{
    //TODOZ: rewrite this with [Flag] attribute
    public enum EditingStateType
    {
        Nothing,                        //Nothing to be expected from a Autocompletiong list
        SmrExpectingType,               //SMR: Expecting a Type name
        SmrExpectingBaseType,           //SMR: Expecting a Base Type for current type
        SmrExpectingOverriden,          //SMR: Expecting a Member name to be overriden
        SmrExpectingTypeOrOverriden,    //SMR: Expecting a Type name or Member name to be overriden
        SmsExpectingMember,             //SMS: Expecting a Member(method or field) for the object in the context (after "." is typed)
        SmsExpectingObject,             //SMS: Expecting an Object Name (member field or locals) that belongs to current sequence
        SmsExpectingObjectOrMember,     //SMS: Expecting an local object or member object/method
        SmsExpectingObjectInContext,    //SMS: those SmsExpectingObject or a member object name that beongs to the type that owns the current invoking method
        SmsExpectingDecendantType,      //SMS: Expecting a concrete Type name (after "<" is typed)
        SmsExpectingAny,                //SMS: Expecting a Type, Object or a Member
    }

    public class EditingState
    {
        private EditingStateType _stateType;

        public EditingStateType StateType
        {
            get { return _stateType; }
            set { _stateType = value; }
        }

        private object _context;

        public object Context
        {
            get { return _context; }
            set { _context = value; }
        }

        public EditingState(EditingStateType stateType, object context)
        {
            _stateType = stateType;
            _context = context;
        }

    }
}
