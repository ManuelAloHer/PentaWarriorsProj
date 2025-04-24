using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEngine;
using UnityEngine.TextCore.Text;


public enum CommandType { None, Move, Attack }

public class Command 
{ 
    public Entity character;
    public Vector3Int selectedGridPoint;
    public CommandType commandType;

    public Command(Entity character, Vector3Int selectedGridPoint, CommandType commandType)
    {
        this.character = character;
        this.selectedGridPoint = selectedGridPoint;
        this.commandType = commandType;
    }
}
public class CommandManager : MonoBehaviour
{
    Command currentCommand;
    public void AddCommand(Entity character, Vector3Int selectedGridPoint, CommandType commandType)
    {
        currentCommand = new Command(character, selectedGridPoint, commandType);
    }

}
