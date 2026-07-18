package com.yourapp.recipes.utils

import android.content.Context
import android.widget.Toast
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import java.text.SimpleDateFormat
import java.util.*

// Date extensions
fun Long.toDisplayDate(): String {
    val sdf = SimpleDateFormat("dd MMMM yyyy", Locale("ru"))
    return sdf.format(Date(this))
}

fun Long.toDayOfWeek(): String {
    val calendar = Calendar.getInstance().apply { timeInMillis = this@toDayOfWeek }
    val days = arrayOf("Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб")
    return days[calendar.get(Calendar.DAY_OF_WEEK) - 1]
}

fun Long.isToday(): Boolean {
    val today = Calendar.getInstance()
    val date = Calendar.getInstance().apply { timeInMillis = this@isToday }
    return today.get(Calendar.DAY_OF_YEAR) == date.get(Calendar.DAY_OF_YEAR) &&
            today.get(Calendar.YEAR) == date.get(Calendar.YEAR)
}

// Number extensions
fun Int.toHoursMinutes(): String {
    val hours = this / 60
    val minutes = this % 60
    return when {
        hours > 0 && minutes > 0 -> "${hours}ч ${minutes}мин"
        hours > 0 -> "${hours}ч"
        else -> "${minutes}мин"
    }
}

fun Float.toDisplayCalories(): String {
    return "${this.toInt()} ккал"
}

// Context extensions
fun Context.showToast(message: String) {
    Toast.makeText(this, message, Toast.LENGTH_SHORT).show()
}

fun Context.showLongToast(message: String) {
    Toast.makeText(this, message, Toast.LENGTH_LONG).show()
}

// Compose extensions
@Composable
fun ErrorMessage(
    message: String,
    modifier: Modifier = Modifier
) {
    Column(modifier = modifier.fillMaxWidth()) {
        Text(
            text = "⚠️ Ошибка",
            style = androidx.compose.material3.MaterialTheme.typography.titleMedium
        )
        Text(
            text = message,
            style = androidx.compose.material3.MaterialTheme.typography.bodyMedium
        )
    }
}

@Composable
fun EmptyState(
    icon: @Composable () -> Unit,
    title: String,
    description: String,
    modifier: Modifier = Modifier
) {
    Column(modifier = modifier.fillMaxWidth()) {
        icon()
        Text(
            text = title,
            style = androidx.compose.material3.MaterialTheme.typography.titleMedium
        )
        Text(
            text = description,
            style = androidx.compose.material3.MaterialTheme.typography.bodyMedium
        )
    }
}
