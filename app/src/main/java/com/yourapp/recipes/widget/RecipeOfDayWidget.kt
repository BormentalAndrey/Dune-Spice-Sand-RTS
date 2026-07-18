package com.yourapp.recipes.widget

import android.app.PendingIntent
import android.appwidget.AppWidgetManager
import android.appwidget.AppWidgetProvider
import android.content.Context
import android.content.Intent
import android.widget.RemoteViews
import com.yourapp.recipes.R
import com.yourapp.recipes.presentation.ui.main.MainActivity

/**
 * Виджет "Рецепт дня" для рабочего стола.
 * Показывает случайный рецепт и обновляется ежедневно.
 */
class RecipeOfDayWidget : AppWidgetProvider() {
    
    override fun onUpdate(
        context: Context,
        appWidgetManager: AppWidgetManager,
        appWidgetIds: IntArray
    ) {
        for (appWidgetId in appWidgetIds) {
            updateAppWidget(context, appWidgetManager, appWidgetId)
        }
    }
    
    override fun onEnabled(context: Context) {
        // Запускаем сервис обновления виджета
        RecipeWidgetUpdateService.startActionUpdateWidgets(context)
    }
    
    override fun onDisabled(context: Context) {
        // Останавливаем сервис обновления
    }
    
    companion object {
        internal fun updateAppWidget(
            context: Context,
            appWidgetManager: AppWidgetManager,
            appWidgetId: Int
        ) {
            val recipeTitle = getRandomRecipeTitle(context)
            
            val intent = Intent(context, MainActivity::class.java).apply {
                flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TOP
                putExtra("widget_action", "open_random_recipe")
            }
            
            val pendingIntent = PendingIntent.getActivity(
                context, 0, intent,
                PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE
            )
            
            val views = RemoteViews(context.packageName, R.layout.recipe_widget).apply {
                setTextViewText(R.id.widget_title, "Рецепт дня")
                setTextViewText(R.id.widget_recipe_title, recipeTitle)
                setOnClickPendingIntent(R.id.widget_container, pendingIntent)
            }
            
            appWidgetManager.updateAppWidget(appWidgetId, views)
        }
        
        private fun getRandomRecipeTitle(context: Context): String {
            // Здесь должна быть логика получения случайного рецепта из БД
            // Пока возвращаем заглушку
            return "Случайный рецепт"
        }
    }
}
