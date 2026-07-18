package com.yourapp.recipes.domain.model

import java.util.*

data class MealPlan(
    val id: Long = 0,
    val recipeId: Long,
    val date: Long,
    val mealType: MealType = MealType.DINNER,
    val servings: Int = 2,
    val notes: String = "",
    val recipe: Recipe? = null
)

enum class MealType(val displayName: String, val icon: String) {
    BREAKFAST("Завтрак", "🌅"),
    LUNCH("Обед", "🌞"),
    DINNER("Ужин", "🌙"),
    SNACK("Перекус", "🍪")
}
