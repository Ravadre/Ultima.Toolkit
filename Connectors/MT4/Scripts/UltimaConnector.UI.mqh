#property copyright "Marcin Deptuła"
#property link      "https://github.com/Ravadre"
#property strict

#include <Arrays/ArrayObj.mqh>
#include <ChartObjects/ChartObjectPanel.mqh>

CArrayObj uiElements;

void CreateVersionLabel(string version)
{
	ObjectCreate("version_label", OBJ_LABEL, 0, 0, 0);
	ObjectSet("version_label", OBJPROP_XDISTANCE, 5);
	ObjectSet("version_label", OBJPROP_YDISTANCE, 13);
	ObjectSetText("version_label", StringConcatenate("Ultima connector ver. ", version), 8, "Arial", White);
	ObjectSetInteger(0, "version_label", OBJPROP_SELECTABLE, false);
}

void CreateDebugPanel() 
{
	const int panelX = 5;
	const int panelY = 30;
   
	string n = "dp_rect_label";
	ObjectCreate(n, OBJ_RECTANGLE_LABEL, 0, 0, 0);
	ObjectSet(n, OBJPROP_XDISTANCE, panelX);
	ObjectSet(n, OBJPROP_YDISTANCE, panelY);
	ObjectSet(n, OBJPROP_XSIZE, 265);
	ObjectSet(n, OBJPROP_YSIZE, 212);
	ObjectSet(n, OBJPROP_BORDER_TYPE, BORDER_FLAT);
	ObjectSet(n, OBJPROP_BGCOLOR, clrDimGray);
	ObjectSet(n, OBJPROP_COLOR, clrSlateGray);
	ObjectSet(n, OBJPROP_WIDTH, 3);
   
	n = "dp_header_label";
	ObjectCreate(n,  OBJ_LABEL, 0, 0, 0);
	ObjectSet(n, OBJPROP_XDISTANCE, panelX + 5);
	ObjectSet(n, OBJPROP_YDISTANCE, panelY + 5);
	ObjectSetText(n, "Debug Panel", 14, "Arial", clrWhite);
	ObjectSetInteger(0, n, OBJPROP_SELECTABLE, false);
	ObjectSetInteger(0, n, OBJPROP_ZORDER, 1);
   
	n = "dp_genticks_label";
	ObjectCreate(n, OBJ_LABEL, 0, 0, 0);
	ObjectSet(n, OBJPROP_XDISTANCE, panelX + 5);
	ObjectSet(n, OBJPROP_YDISTANCE, panelY + 35);
	ObjectSetText(n, "", 12, "Arial", clrWhite);
	ObjectSetInteger(0, n, OBJPROP_SELECTABLE, false);
	
	n = "dp_genticks_btn";
	ObjectCreate(n, OBJ_BUTTON, 0, 0, 0);
	ObjectSet(n, OBJPROP_XDISTANCE, panelX + 200);
	ObjectSet(n, OBJPROP_YDISTANCE, panelY + 30);
	ObjectSetText(n, "", 12, "Arial", clrBlack);
	ObjectSet(n, OBJPROP_COLOR, clrBlack);  
	ObjectSetInteger(0, n, OBJPROP_SELECTABLE, false);
	ObjectSet(n, OBJPROP_XSIZE, 60);
	ObjectSet(n, OBJPROP_YSIZE, 30);	
	ObjectSetInteger(0, n, OBJPROP_ZORDER, 2);
	
	n = "dp_lockticks_label";
	ObjectCreate(n, OBJ_LABEL, 0, 0, 0);
	ObjectSet(n, OBJPROP_XDISTANCE, panelX + 5);
	ObjectSet(n, OBJPROP_YDISTANCE, panelY + 75);
	ObjectSetText(n, "", 12, "Arial", clrWhite);
	ObjectSetInteger(0, n, OBJPROP_SELECTABLE, false);
	
	n = "dp_lockticks_btn";
	ObjectCreate(n, OBJ_BUTTON, 0, 0, 0);
	ObjectSet(n, OBJPROP_XDISTANCE, panelX + 200);
	ObjectSet(n, OBJPROP_YDISTANCE, panelY + 70);
	ObjectSetText(n, "", 12, "Arial", clrBlack);
	ObjectSet(n, OBJPROP_COLOR, clrBlack);  
	ObjectSetInteger(0, n, OBJPROP_SELECTABLE, false);
	ObjectSet(n, OBJPROP_XSIZE, 60);
	ObjectSet(n, OBJPROP_YSIZE, 30);	
	ObjectSetInteger(0, n, OBJPROP_ZORDER, 2);
  
  	n = "dp_priceoffset_label";
	ObjectCreate(n, OBJ_LABEL, 0, 0, 0);
	ObjectSet(n, OBJPROP_XDISTANCE, panelX + 5);
	ObjectSet(n, OBJPROP_YDISTANCE, panelY + 120);
	ObjectSetText(n, "", 12, "Arial", clrWhite);
	ObjectSetInteger(0, n, OBJPROP_SELECTABLE, false);
	
	n = "dp_priceoffset_up_btn";
	ObjectCreate(n, OBJ_BUTTON, 0, 0, 0);
	ObjectSet(n, OBJPROP_XDISTANCE, panelX + 200);
	ObjectSet(n, OBJPROP_YDISTANCE, panelY + 110);
	ObjectSetString(0, n, OBJPROP_FONT, "Consolas");
	ObjectSet(n, OBJPROP_FONTSIZE, 16);
	ObjectSetString(0, n, OBJPROP_TEXT, "+");
	ObjectSet(n, OBJPROP_COLOR, clrBlack);  
	ObjectSetInteger(0, n, OBJPROP_SELECTABLE, false);
	ObjectSet(n, OBJPROP_XSIZE, 60);
	ObjectSet(n, OBJPROP_YSIZE, 21);	
	ObjectSetInteger(0, n, OBJPROP_ZORDER, 2);
	
	n = "dp_priceoffset_down_btn";
	ObjectCreate(n, OBJ_BUTTON, 0, 0, 0);
	ObjectSet(n, OBJPROP_XDISTANCE, panelX + 200);
	ObjectSet(n, OBJPROP_YDISTANCE, panelY + 131);
	ObjectSetString(0, n, OBJPROP_FONT, "Consolas");
	ObjectSet(n, OBJPROP_FONTSIZE, 16);
	ObjectSetString(0, n, OBJPROP_TEXT, "-");
	ObjectSet(n, OBJPROP_COLOR, clrBlack);  
	ObjectSetInteger(0, n, OBJPROP_SELECTABLE, false);
	ObjectSet(n, OBJPROP_XSIZE, 60);
	ObjectSet(n, OBJPROP_YSIZE, 21);	
	ObjectSetInteger(0, n, OBJPROP_ZORDER, 2);
	
	n = "dp_pricespread_label";
	ObjectCreate(n, OBJ_LABEL, 0, 0, 0);
	ObjectSet(n, OBJPROP_XDISTANCE, panelX + 5);
	ObjectSet(n, OBJPROP_YDISTANCE, panelY + 172);
	ObjectSetText(n, "", 12, "Arial", clrWhite);
	ObjectSetInteger(0, n, OBJPROP_SELECTABLE, false);
	
	n = "dp_pricespread_up_btn";
	ObjectCreate(n, OBJ_BUTTON, 0, 0, 0);
	ObjectSet(n, OBJPROP_XDISTANCE, panelX + 200);
	ObjectSet(n, OBJPROP_YDISTANCE, panelY + 162);
	ObjectSetString(0, n, OBJPROP_FONT, "Consolas");
	ObjectSet(n, OBJPROP_FONTSIZE, 16);
	ObjectSetString(0, n, OBJPROP_TEXT, "+");
	ObjectSet(n, OBJPROP_COLOR, clrBlack);  
	ObjectSetInteger(0, n, OBJPROP_SELECTABLE, false);
	ObjectSet(n, OBJPROP_XSIZE, 60);
	ObjectSet(n, OBJPROP_YSIZE, 21);	
	ObjectSetInteger(0, n, OBJPROP_ZORDER, 2);
	
	n = "dp_pricespread_down_btn";
	ObjectCreate(n, OBJ_BUTTON, 0, 0, 0);
	ObjectSet(n, OBJPROP_XDISTANCE, panelX + 200);
	ObjectSet(n, OBJPROP_YDISTANCE, panelY + 183);
	ObjectSetString(0, n, OBJPROP_FONT, "Consolas");
	ObjectSet(n, OBJPROP_FONTSIZE, 16);
	ObjectSetString(0, n, OBJPROP_TEXT, "-");
	ObjectSet(n, OBJPROP_COLOR, clrBlack);  
	ObjectSetInteger(0, n, OBJPROP_SELECTABLE, false);
	ObjectSet(n, OBJPROP_XSIZE, 60);
	ObjectSet(n, OBJPROP_YSIZE, 21);	
	ObjectSetInteger(0, n, OBJPROP_ZORDER, 2);
}

void RefreshDebugState() 
{
	if (debugState.IsGeneratingTicks) 
	{
		ObjectSetText("dp_genticks_label", "Generating ticks: Yes", 12, "Arial", White);
		ObjectSetText("dp_genticks_btn", "Stop", 12, "Arial", clrWhite);
	}
	else
	{
		ObjectSetText("dp_genticks_btn", "Start", 12, "Arial", clrWhite);
		ObjectSetText("dp_genticks_label", "Generating ticks: No", 12, "Arial", White);
	}
	
	if (debugState.GenerateInLock) 
	{
		ObjectSetText("dp_lockticks_label", "Lock generated ticks: Yes", 12, "Arial", White);
		ObjectSetText("dp_lockticks_btn", "Unlock", 12, "Arial", clrWhite);
	}
	else
	{
		ObjectSetText("dp_lockticks_btn", "Lock", 12, "Arial", clrWhite);
		ObjectSetText("dp_lockticks_label", "Lock generated ticks: No", 12, "Arial", White);
	}
	
	ObjectSetText("dp_priceoffset_label", StringConcatenate("Price offset: ", debugState.PriceOffset),
		12, "Arial", clrWhite);
	ObjectSetText("dp_pricespread_label", StringConcatenate("Price spread: ", debugState.PriceSpread),
		12, "Arial", clrWhite);
}

void UnlockButtons()
{
	ObjectSetInteger(0, "dp_genticks_btn", OBJPROP_STATE, false);
	ObjectSetInteger(0, "dp_lockticks_btn", OBJPROP_STATE, false);
	ObjectSetInteger(0, "dp_priceoffset_up_btn", OBJPROP_STATE, false);
	ObjectSetInteger(0, "dp_priceoffset_down_btn", OBJPROP_STATE, false);
	ObjectSetInteger(0, "dp_pricespread_up_btn", OBJPROP_STATE, false);
	ObjectSetInteger(0, "dp_pricespread_down_btn", OBJPROP_STATE, false);
}

void CreateUI() 
{
	CreateVersionLabel(VERSION);  
	
	if (debugState.IsDebugging) 
	{
		CreateDebugPanel();
	}
	
	ChartRedraw();
}

void DestroyUI() 
{
	int t = ObjectsTotal();
	
	for(int i = t - 1; i >= 0; --i)
	{
		string name = ObjectName(i);
		ObjectDelete(name);
	}
	
	ChartRedraw();
}