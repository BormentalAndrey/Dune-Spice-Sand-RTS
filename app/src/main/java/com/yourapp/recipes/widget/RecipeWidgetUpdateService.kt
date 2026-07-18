package com.yourapp.recipes.widget

import android.app.Service
import android.appwidget.AppWidgetManager
import android.content.Intent
import android.os.IBinder
import androidx.core.app.JobIntentService

class RecipeWidgetUpdateService : JobIntentService() {
    
    override fun onHandleWork(intent: Intent) {
        val appWidgetManager = AppWidgetManager.getInstance(this)
        val appWidgetIds = appWidgetManager.getAppWidgetIds(
            Intent(this, RecipeOfDayWidget::class.java).component
        )
        
        for (appWidgetId in appWidgetIds) {
            RecipeOfDayWidget.updateAppWidget(this, appWidgetManager, appWidgetId)
        }
    }
    
    companion object {
        private const val JOB_ID = 1000
        
        fun startActionUpdateWidgets(context: Context) {
            val intent = Intent(context, RecipeWidgetUpdateService::class.java)
            enqueueWork(context, RecipeWidgetUpdateService::class.java, JOB_ID, intent)
        }
    }
}
