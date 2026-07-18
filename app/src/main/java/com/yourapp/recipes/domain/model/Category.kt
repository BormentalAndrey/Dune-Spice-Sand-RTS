package com.yourapp.recipes.domain.model

enum class Category(val displayName: String, val icon: String) {
    BREAKFAST("Завтрак", "🌅"),
    LUNCH("Обед", "🌞"),
    DINNER("Ужин", "🌙"),
    DESSERT("Десерт", "🍰"),
    SALAD("Салат", "🥗"),
    SOUP("Суп", "🍲"),
    SNACK("Закуска", "🥪"),
    DRINK("Напиток", "🍹")
}
