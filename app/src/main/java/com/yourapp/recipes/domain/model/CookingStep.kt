package com.yourapp.recipes.domain.model

data class CookingStep(
    val stepNumber: Int,
    val description: String,
    val durationMinutes: Int = 0,
    val isCompleted: Boolean = false
)
